using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.DeepLinking;
using LtiAdvantage.Lti;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Handles LTI Advantage authorization requests:
    ///  1. Replaces the cookie principal's subject with the person identified by
    ///     login_hint (impersonation) so the id_token carries the impersonated user.
    ///  2. Appends LTI Advantage claims (resource link / deep linking) to that
    ///     principal so they end up on the id_token.
    /// Scoped to authorization-endpoint sign-ins only — token-endpoint sign-ins
    /// (e.g. password grant) flow through unmodified.
    /// </summary>
    public class LtiAdvantageClaimsHandler : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessSignInContext>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<LtiAdvantageClaimsHandler> _logger;

        public LtiAdvantageClaimsHandler(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            ILogger<LtiAdvantageClaimsHandler> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        public async ValueTask HandleAsync(OpenIddictServerEvents.ProcessSignInContext context)
        {
            if (context.Principal == null || context.Request == null)
            {
                return;
            }

            // Only act on authorization requests.
            if (context.EndpointType != OpenIddictServerEndpointType.Authorization)
            {
                return;
            }

            var loginHint = context.Request.LoginHint;
            var subject = context.Principal.FindFirstValue(OpenIddictConstants.Claims.Subject);
            if (!string.IsNullOrEmpty(loginHint) && subject != loginHint)
            {
                _logger.LogInformation("Impersonating subject {Subject}.", loginHint);

                // Swap the user claims in place. Replacing the whole principal would
                // drop OpenIddict's internal claims (scopes, presenters → id_token aud).
                var currentIdentity = (ClaimsIdentity)context.Principal.Identity!;
                foreach (var claim in currentIdentity.Claims
                             .Where(c => c.Type is OpenIddictConstants.Claims.Subject
                                 or OpenIddictConstants.Claims.Name
                                 or OpenIddictConstants.Claims.Email)
                             .ToList())
                {
                    currentIdentity.RemoveClaim(claim);
                }

                currentIdentity.AddClaims(new[]
                {
                    new Claim(OpenIddictConstants.Claims.Subject, loginHint)
                        .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
                    new Claim(OpenIddictConstants.Claims.AuthenticationTime,
                            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                            ClaimValueTypes.Integer64)
                        .SetDestinations(OpenIddictConstants.Destinations.IdentityToken),
                    new Claim("idp", "local")
                        .SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
                });
            }

            var ltiMessageHint = (string)context.Request.GetParameter("lti_message_hint");
            if (string.IsNullOrWhiteSpace(ltiMessageHint))
            {
                _logger.LogInformation("Not an LTI authorization request.");
                return;
            }

            if (!int.TryParse(loginHint, out var personId))
            {
                _logger.LogError("Cannot convert login hint '{LoginHint}' to person id.",
                    loginHint);
                return;
            }

            JsonElement message;
            try
            {
                message = JsonSerializer.Deserialize<JsonElement>(ltiMessageHint);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Cannot parse lti_message_hint as JSON: {Hint}", ltiMessageHint);
                return;
            }

            var id = message.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var idValue)
                ? idValue
                : 0;
            var messageType = message.TryGetProperty("messageType", out var msgTypeEl)
                ? msgTypeEl.GetString()
                : null;
            var hasCourseId = message.TryGetProperty("courseId", out var courseEl)
                              && courseEl.ValueKind != JsonValueKind.Null
                              && !string.IsNullOrEmpty(courseEl.ToString());

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is not available.");
                return;
            }

            // Application user that owns the current launch context. The cookie
            // principal carrying this still lives on httpContext.User even after
            // OpenIddict swapped the OIDC principal to the impersonated person.
            var user = await _context.GetUserLightAsync(httpContext.User);
            if (user == null)
            {
                _logger.LogError("Application user not found for cookie principal.");
                return;
            }

            var course = hasCourseId ? user.Course : null;
            var person = await _context.GetPersonAsync(personId);
            if (person == null)
            {
                _logger.LogError("Person {PersonId} not found.", personId);
                return;
            }

            List<Claim> claims;
            switch (messageType)
            {
                case Constants.Lti.LtiResourceLinkRequestMessageType:
                {
                    var resourceLink = await _context.GetResourceLinkAsync(id);
                    if (resourceLink == null)
                    {
                        _logger.LogError("Resource link {ResourceLinkId} not found.", id);
                        return;
                    }

                    var gradebookColumn = await _context.GetGradebookColumnByResourceLinkIdAsync(id);
                    claims = GetResourceLinkRequestClaims(resourceLink, gradebookColumn, person, course, user.Platform);
                    break;
                }
                case Constants.Lti.LtiDeepLinkingRequestMessageType:
                {
                    var tool = await _context.GetToolAsync(id);
                    if (tool == null)
                    {
                        _logger.LogError("Tool {ToolId} not found.", id);
                        return;
                    }

                    claims = GetDeepLinkingRequestClaims(tool, person, course, user.Platform);
                    break;
                }
                default:
                    _logger.LogError("messageType=\"{MessageType}\" not supported.", messageType);
                    return;
            }

            // Append LTI claims to the principal so they end up on the id_token.
            var identity = (ClaimsIdentity)context.Principal.Identity!;
            foreach (var claim in claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.IdentityToken);
                identity.AddClaim(claim);
            }
        }

        private List<Claim> GetDeepLinkingRequestClaims(
            Tool tool,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext!.Request;

            var request = new LtiDeepLinkingRequest
            {
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    // Both UIs run the deep linking flow in an iframe (a modal dialog).
                    DocumentTarget = DocumentTarget.Iframe,
                    Locale = CultureInfo.CurrentUICulture.Name
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                Lti11LegacyUserId = person.Id.ToString(),
                Platform = new PlatformClaimValueType
                {
                    ContactEmail = platform.ContactEmail,
                    Description = platform.Description,
                    Guid = platform.Id.ToString(),
                    Name = platform.Name,
                    ProductFamilyCode = platform.ProductFamilyCode,
                    Url = platform.Url,
                    Version = platform.Version
                },
                Roles = PeopleModel.ParsePersonRoles(person.Roles),
                TargetLinkUri = tool.DeepLinkingLaunchUrl
            };

            if (course == null)
            {
                request.Roles = request.Roles.Where(r => !r.ToString().StartsWith("Context")).ToArray();
            }
            else
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id.ToString(),
                    Title = course.Name,
                    Type = new[] { ContextType.CourseSection }
                };
            }

            request.DeepLinkingSettings = new DeepLinkingSettingsClaimValueType
            {
                // The platform can open LTI links and plain links in a new window or embed
                // them in an iframe; the item's "iframe" or "window" property picks one.
                AcceptPresentationDocumentTargets = new[] { DocumentTarget.Iframe, DocumentTarget.Window },
                AcceptMultiple = true,
                // Besides LTI links the platform shows links, HTML fragments, files and images.
                AcceptTypes = new[]
                {
                    Constants.ContentItemTypes.LtiLink,
                    Constants.ContentItemTypes.Link,
                    Constants.ContentItemTypes.Html,
                    Constants.ContentItemTypes.File,
                    Constants.ContentItemTypes.Image
                },
                AutoCreate = true,
                DeepLinkReturnUrl = _linkGenerator.GetUriByPage(
                    "/DeepLinks",
                    handler: null,
                    values: new { platformId = platform.Id, courseId = course?.Id },
                    scheme: httpRequest.Scheme,
                    host: httpRequest.Host)
            };

            if (tool.CustomProperties.TryConvertToDictionary(out var custom))
            {
                var substitutions = new CustomPropertySubstitutions
                {
                    LtiUser = new LtiUser { Username = person.Username }
                };
                request.Custom = substitutions.ReplaceCustomPropertyValues(custom);
            }

            return new List<Claim>(request.Claims);
        }

        private List<Claim> GetResourceLinkRequestClaims(
            ResourceLink resourceLink,
            GradebookColumn gradebookColumn,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext!.Request;

            var request = new LtiResourceLinkRequest
            {
                DeploymentId = resourceLink.Tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    // Tells the tool how it is shown, e.g. so it can hide its own chrome in an iframe.
                    DocumentTarget = resourceLink.DocumentTarget == "iframe" ? DocumentTarget.Iframe : DocumentTarget.Window,
                    Height = resourceLink.DocumentTarget == "iframe" ? resourceLink.IframeHeight : null,
                    Locale = CultureInfo.CurrentUICulture.Name,
                    ReturnUrl = $"{httpRequest.Scheme}://{httpRequest.Host}"
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                Lti11LegacyUserId = person.Id.ToString(),
                Platform = new PlatformClaimValueType
                {
                    ContactEmail = platform.ContactEmail,
                    Description = platform.Description,
                    Guid = platform.Id.ToString(),
                    Name = platform.Name,
                    ProductFamilyCode = platform.ProductFamilyCode,
                    Url = platform.Url,
                    Version = platform.Version
                },
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Id = resourceLink.Id.ToString(),
                    Title = resourceLink.Title,
                    Description = resourceLink.Description
                },
                Roles = PeopleModel.ParsePersonRoles(person.Roles),
                TargetLinkUri = resourceLink.Tool.LaunchUrl
            };

            if (course == null)
            {
                request.Roles = request.Roles.Where(r => !r.ToString().StartsWith("Context")).ToArray();
            }
            else
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id.ToString(),
                    Title = course.Name,
                    Type = new[] { ContextType.CourseSection }
                };

                request.AssignmentGradeServices = new AssignmentGradeServicesClaimValueType
                {
                    Scope = new List<string> { Constants.LtiScopes.Ags.LineItem },
                    LineItemUrl = gradebookColumn == null
                        ? null
                        : _linkGenerator.GetUriByRouteValues(
                            Constants.ServiceEndpoints.Ags.LineItemService,
                            new { contextId = course.Id, lineItemId = gradebookColumn.Id },
                            httpRequest.Scheme,
                            httpRequest.Host),
                    LineItemsUrl = _linkGenerator.GetUriByRouteValues(
                        Constants.ServiceEndpoints.Ags.LineItemsService,
                        new { contextId = course.Id },
                        httpRequest.Scheme,
                        httpRequest.Host)
                };

                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl = _linkGenerator.GetUriByRouteValues(
                        Constants.ServiceEndpoints.Nrps.MembershipService,
                        new { contextId = course.Id },
                        httpRequest.Scheme,
                        httpRequest.Host)
                };
            }

            if (!resourceLink.Tool.CustomProperties.TryConvertToDictionary(out var custom))
            {
                custom = new Dictionary<string, string>();
            }
            if (resourceLink.CustomProperties.TryConvertToDictionary(out var linkDictionary))
            {
                foreach (var property in linkDictionary)
                {
                    custom[property.Key] = property.Value;
                }
            }

            var substitutions = new CustomPropertySubstitutions
            {
                LtiUser = new LtiUser { Username = person.Username }
            };
            request.Custom = substitutions.ReplaceCustomPropertyValues(custom);

            return new List<Claim>(request.Claims);
        }
    }

    /// <summary>
    /// Registers <see cref="LtiAdvantageClaimsHandler"/> with the OpenIddict server pipeline.
    /// </summary>
    public static class LtiAdvantageClaimsHandlerExtensions
    {
        public static OpenIddictServerBuilder AddLtiAdvantageClaims(this OpenIddictServerBuilder builder)
        {
            builder.Services.TryAddScoped<LtiAdvantageClaimsHandler>();
            // Must run before PrepareIdentityTokenPrincipal, which copies the principal's
            // claims into the id_token. Anything added after that step never reaches the tool.
            builder.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(
                descriptor => descriptor
                    .UseScopedHandler<LtiAdvantageClaimsHandler>()
                    .SetOrder(OpenIddictServerHandlers.PrepareIdentityTokenPrincipal.Descriptor.Order - 1));
            return builder;
        }
    }
}
