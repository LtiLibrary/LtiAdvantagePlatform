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
using LtiAdvantage.Lti;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

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
            if (context.ValidatedRequest is ValidatedAuthorizeRequest request)
            {
                _logger.LogInformation($"Starting {nameof(GetProfileDataAsync)}.");

                var ltiMessageHint = request.Raw["lti_message_hint"];
                if (!int.TryParse(ltiMessageHint, out var resourceLinkId))
                {
                    _logger.LogError("lti_message_hint is not an int.");
                    return;
                }

                var resourceLink = await _context.GetResourceLinkAsync(resourceLinkId);
                if (resourceLink == null)
                {
                    _logger.LogError($"Cannot find resource link [{resourceLinkId}].");
                    return;
                }

                // Null unless there is exactly one gradebook column for the resource link.
                var gradebookColumn = await _context.GetGradebookColumnByResourceLinkAsync(resourceLinkId);

                var tool = resourceLink.Tool;
                if (tool == null)
                {
                    _logger.LogError("Cannot find tool.");
                    return;
                }

                var person = await _context.GetPersonAsync(request.LoginHint);
                if (person == null)
                {
                    _logger.LogError($"Cannot find person [{request.LoginHint}].");
                    return;
                }

                var course = await _context.GetCourseByResourceLinkAsync(resourceLink.Id);

                var user = await _context.GetUserByResourceLinkAsync(resourceLink.Id);
                if (user == null)
                {
                    _logger.LogError("Cannot find user.");
                    return;
                }

                context.IssuedClaims = GetLtiClaimsAsync(resourceLink, gradebookColumn, tool, person, course, user.Platform);
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
        /// Returns a list LTI Advantage claims.
        /// </summary>
        /// <param name="resourceLink">The resource link.</param>
        /// <param name="gradebookColumn">The gradebool column for this resource link.</param>
        /// <param name="tool">The tool.</param>
        /// <param name="person">The person being authorized.</param>
        /// <param name="course">The course (can be null).</param>
        /// <param name="platform">The platform.</param>
        /// <returns></returns>
        private List<Claim> GetLtiClaimsAsync(
            ResourceLink resourceLink,
            GradebookColumn gradebookColumn,
            Tool tool,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            var request = new LtiResourceLinkRequest
            {
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.Iframe,
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
                    Title = resourceLink.Title
                },
                TargetLinkUri = tool.LaunchUrl
            };

            // Add the context if the launch is from a course
            // (e.g. an assignment). Leave it blank if the launch
            // is from outside of a course (e.g. a system menu).
            if (course != null)
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id.ToString(),
                    Title = course.Name,
                    Type = new[] { ContextType.CourseSection }
                };

                // Only include context roles if the launch includes
                // a context.
                request.Roles = PeopleModel.ParsePersonRoles(person.Roles);

                // Only include the Assignment and Grade Services claim if the launch includes a context.
                request.AssignmentGradeServices = new AssignmentGradeServicesClaimValueType
                {
                    Scope = new List<string>
                    {
                        Constants.LtiScopes.AgsLineItem
                    },
                    LineItemUrl = gradebookColumn == null ? null : _linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.AgsLineItemService,
                        new {contextId = course.Id, gradebookColumn.Id}, httpRequest.Scheme, httpRequest.Host),
                    LineItemsUrl = _linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.AgsLineItemsService,
                        new {contextId = course.Id}, httpRequest.Scheme, httpRequest.Host)
                };

                // Only include Names and Role Provisioning Service claim if the launch includes a context.
                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl =_linkGenerator.GetUriByRouteValues(Constants.ServiceEndpoints.NrpsMembershipService,
                        new {contextId = course.Id}, httpRequest.Scheme, httpRequest.Host)
                };
            }
            else
            {
                var roles = PeopleModel.ParsePersonRoles(person.Roles);
                request.Roles = roles.Where(r => !r.ToString().StartsWith("Context")).ToArray();
            }

            // Collect custom properties
            tool.CustomProperties.TryConvertToDictionary(out var custom);
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
