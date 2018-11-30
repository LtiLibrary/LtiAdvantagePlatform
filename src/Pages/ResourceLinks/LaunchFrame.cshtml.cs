using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.Lti;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchFrameModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly IdentityServerTools _tools;
        private readonly ILogger<LaunchFrameModel> _logger;

        public LaunchFrameModel(
            ApplicationDbContext context, 
            IConfigurationDbContext identityContext,
            IdentityServerTools tools,
            ILogger<LaunchFrameModel> logger)
        {
            _context = context;
            _identityContext = identityContext;
            _tools = tools;
            _logger = logger;
        }

        public string IdToken { get; private set; }
        public string ToolUrl { get; private set; }

        /// <summary>
        /// This page is the source for an iframe inside of LaunchPage.
        /// </summary>
        /// <param name="id">The <see cref="ResourceLink"/>.</param>
        /// <param name="personId">The <see cref="Person"/> ID launching the resource.</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int? id, string personId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var resourceLink = await _context.ResourceLinks
                .Include(l => l.Tool)
                .SingleOrDefaultAsync(l => l.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            var tool = resourceLink.Tool;
            if (tool == null)
            {
                return NotFound();
            }

            var person = user.People.SingleOrDefault(p => p.Id == personId);
            if (person == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);
            if (client == null)
            {
                return NotFound();
            }

            var course = user.Course.ResourceLinks.Any(l => l.Id == resourceLink.Id)
                ? user.Course
                : null;

            var platform = user.Platform;

            IdToken = await GetJwtAsync(resourceLink, tool, client, person, course, platform);
            ToolUrl = tool.LaunchUrl;

            _logger.LogInformation($"Launching tool at {ToolUrl}.");

            return Page();
        }

        private async Task<string> GetJwtAsync(
            ResourceLink resourceLink, 
            Tool tool, 
            Client client, 
            Person person, 
            Course course, 
            Platform platform)
        {
            var request = new LtiResourceLinkRequest
            {
                Audiences = new [] { client.ClientId },
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.Iframe,
                    Locale = CultureInfo.CurrentUICulture.Name,
                    ReturnUrl = Request.GetDisplayUrl()
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                Nonce = LtiResourceLinkRequest.GenerateCryptographicNonce(),
                Platform = new PlatformClaimValueType
                {
                    ContactEmail = platform.ContactEmail,
                    Description = platform.Description,
                    Guid = platform.Id,
                    Name = platform.Name,
                    ProductFamilyCode = platform.ProductFamilyCode,
                    Url = Request.GetDisplayUrl(),
                    Version = platform.Version
                },
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Id = resourceLink.Id.ToString(),
                    Title = resourceLink.Title
                },
                UserId = person.Id
            };

            // Add the context if the launch is from a course
            // (e.g. an assignment). Leave it blank if the launch
            // is from outside of a course (e.g. a system menu).
            if (course != null)
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id,
                    Title = course.Name,
                    Type = new[] {ContextType.CourseSection}
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
                    LineItem = Url.RouteUrl(Constants.ServiceEndpoints.AgsLineItemService,
                        new { contextId = course.Id, id = resourceLink.Id.ToString() },
                        "https",
                        Request.Host.ToString()),
                    LineItems = Url.RouteUrl(Constants.ServiceEndpoints.AgsLineItemService,
                        new { contextId = course.Id },
                        "https",
                        Request.Host.ToString())
                };

                // Only include Names and Role Provisioning Service claim if the launch includes a context.
                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl = 
                        Url.RouteUrl(Constants.ServiceEndpoints.NrpsMembershipService, 
                            new { contextId = course.Id },
                            "https",
                            Request.Host.ToString())
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

            _logger.LogInformation($"Payload: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

            return await _tools.IssueJwtAsync(3600, request.Claims);
        }
     }
}