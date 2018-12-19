using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.DeepLinking;
using LtiAdvantage.IdentityServer4;
using LtiAdvantage.Lti;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace AdvantagePlatform.Utility
{
    /// <inheritdoc />
    /// <summary>
    /// Custom ProfileService to add LTI Advantage claims to id_token.
    /// </summary>
    /// <remarks>
    /// See https://damienbod.com/2016/11/18/extending-identity-in-identityserver4-to-manage-users-in-asp-net-core/.
    /// </remarks>
    public class LtiAdvantageProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;
        private readonly ILogger<LtiAdvantageProfileService> _logger;

        public LtiAdvantageProfileService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator,
            ILogger<LtiAdvantageProfileService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add LTI Advantage claims to id_token.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                _logger.LogInformation($"Starting {nameof(GetProfileDataAsync)}.");

                if (context.ValidatedRequest is ValidatedAuthorizeRequest request)
                {
                    var ltiMessageHint = request.Raw["lti_message_hint"];
                    if (ltiMessageHint.IsMissing())
                    {
                        _logger.LogInformation("Not an LTI request.");
                        return;
                    }

                    if (!int.TryParse(request.LoginHint, out var personId))
                    {
                        _logger.LogError("Cannot convert login hint to person id.");
                    }

                    var message = JToken.Parse(ltiMessageHint);
                    var id = message.Value<int>("id");
                    var user = await _context.GetUserAsync(_httpContextAccessor.HttpContext.User);
                    var course = message.Value<string>("courseId") == null ? null : user.Course;
                    var person = await _context.GetPersonAsync(personId);
                    var messageType = message.Value<string>("messageType");

                    switch (messageType)
                    {
                        case Constants.Lti.LtiResourceLinkRequestMessageType:
                        {
                            var resourceLink = await _context.GetResourceLinkAsync(id);

                            // Null unless there is exactly one gradebook column for the resource link.
                            var gradebookColumn = await _context.GetGradebookColumnByResourceLinkIdAsync(id);

                            context.IssuedClaims = GetResourceLinkRequestClaims(
                                resourceLink, gradebookColumn, person, course, user.Platform);

                            break;
                        }
                        case Constants.Lti.LtiDeepLinkingRequestMessageType:
                        {
                            var tool = await _context.GetToolAsync(id);

                            context.IssuedClaims = GetDeepLinkingRequestClaims(
                                tool, person, course, user.Platform);

                            break;
                        }
                        default:
                            _logger.LogError($"{nameof(messageType)}=\"{messageType}\" not supported.");

                            break;
                    }
                }
            }
            finally
            {
                _logger.LogInformation($"Exiting {nameof(GetProfileDataAsync)}.");
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns the LTI claims for an LtiDeepLinkingRequest.
        /// </summary>
        /// <param name="tool">The deep linking tool.</param>
        /// <param name="person">The person being authorized.</param>
        /// <param name="course">The course (can be null).</param>
        /// <param name="platform">The platform.</param>
        /// <returns></returns>
        private List<Claim> GetDeepLinkingRequestClaims(
            Tool tool,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            var request = new LtiDeepLinkingRequest
            {
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.Window,
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

            // Add the context if the launch is from a course.
            if (course == null)
            {
                // Remove context roles
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

            // Add the deep linking settings
            request.DeepLinkingSettings = new DeepLinkingSettingsClaimValueType
            {
                AcceptPresentationDocumentTargets = new [] { DocumentTarget.Window },
                AcceptMultiple = true,
                AcceptTypes = new [] { Constants.ContentItemTypes.LtiLink },
                AutoCreate = true,
                DeepLinkReturnUrl = _linkGenerator.GetUriByPage(
                    "/DeepLinks", 
                    handler: null, 
                    values: new {platformId = platform.Id, courseId = course?.Id}, 
                    scheme: httpRequest.Scheme, 
                    host: httpRequest.Host)
            };

            // Collect custom properties
            if (tool.CustomProperties.TryConvertToDictionary(out var custom))
            {
                // Prepare for custom property substitutions
                var substitutions = new CustomPropertySubstitutions
                {
                    LtiUser = new LtiUser
                    {
                        Username = person.Username
                    }
                };

                request.Custom = substitutions.ReplaceCustomPropertyValues(custom);
            }

            return new List<Claim>(request.Claims);
        }

        /// <summary>
        /// Returns the LTI claims for an LtiResourceLinkRequest.
        /// </summary>
        /// <param name="resourceLink">The resource link.</param>
        /// <param name="gradebookColumn">The gradebool column for this resource link.</param>
        /// <param name="person">The person being authorized.</param>
        /// <param name="course">The course (can be null).</param>
        /// <param name="platform">The platform.</param>
        /// <returns></returns>
        private List<Claim> GetResourceLinkRequestClaims(
            ResourceLink resourceLink,
            GradebookColumn gradebookColumn,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            var request = new LtiResourceLinkRequest
            {
                DeploymentId = resourceLink.Tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.Window,
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

            // Add the context if the launch is from a course.
            if (course == null)
            {
                // Remove context roles
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
                    Scope = new List<string>
                    {
                        Constants.LtiScopes.AgsLineItem
                    },
                    LineItemUrl = gradebookColumn == null ? null : _linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.AgsLineItemService,
                        new { contextId = course.Id, lineItemId = gradebookColumn.Id }, httpRequest.Scheme, httpRequest.Host),
                    LineItemsUrl = _linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.AgsLineItemsService,
                        new { contextId = course.Id }, httpRequest.Scheme, httpRequest.Host)
                };

                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl = _linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.NrpsMembershipService,
                        new { contextId = course.Id }, httpRequest.Scheme, httpRequest.Host)
                };
            }

            // Collect custom properties
            if (!resourceLink.Tool.CustomProperties.TryConvertToDictionary(out var custom))
            {
                custom = new Dictionary<string, string>();
            }
            if (resourceLink.CustomProperties.TryConvertToDictionary(out var linkDictionary))
            {
                foreach (var property in linkDictionary)
                {
                    if (custom.ContainsKey(property.Key))
                    {
                        custom[property.Key] = property.Value;
                    }
                    else
                    {
                        custom.Add(property.Key, property.Value);
                    }
                }
            }

            // Prepare for custom property substitutions
            var substitutions = new CustomPropertySubstitutions
            {
                LtiUser = new LtiUser
                {
                    Username = person.Username
                }
            };

            request.Custom = substitutions.ReplaceCustomPropertyValues(custom);

            return new List<Claim>(request.Claims);
        }
    }
}
