using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantageLibrary;
using LtiAdvantageLibrary.Lti;
using LtiAdvantageLibrary.NamesRoleService;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchFrameModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly IdentityServerTools _tools;

        public LaunchFrameModel(
            ApplicationDbContext context, 
            IConfigurationDbContext identityContext,
            IdentityServerTools tools)
        {
            _context = context;
            _identityContext = identityContext;
            _tools = tools;
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

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
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

            var course = resourceLink.LinkContext == ResourceLink.LinkContexts.Course
                ? user.Course
                : null;

            var platform = user.Platform;

            var custom = new Dictionary<string, string> {{"myCustomValue", "123"}, {"username", "$User.username"}};

            IdToken = await GetJwtAsync(resourceLink, tool, client, person, course, platform, custom);
            ToolUrl = tool.LaunchUrl;

            return Page();
        }

        private async Task<string> GetJwtAsync(
            ResourceLink resourceLink, 
            Tool tool, 
            Client client, 
            Person person, 
            Course course, 
            Platform platform,
            Dictionary<string, string> custom
            )
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
                    ReturnUrl = Request.GetDisplayUrl()
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                Locale = CultureInfo.CurrentUICulture.Name,
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
                request.Roles = Areas.Identity.Pages.Account.Manage.PeopleModel.ParsePersonRoles(person.Roles);

                // Only include Names and Role Provisioning Service claim if
                // the launch includes a context.
                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl = 
                        Url.RouteUrl(Constants.LtiClaims.NamesRoleService, 
                            new { contextId = course.Id },
                            "https",
                            Request.Host.ToString())
                };
            }
            else
            {
                var roles = Areas.Identity.Pages.Account.Manage.PeopleModel.ParsePersonRoles(person.Roles);
                request.Roles = roles.Where(r => !r.ToString().StartsWith("Context")).ToArray();
            }

            // Add the custom parameters
            request.UserName = $"{request.GivenName.Substring(0, 1)}{request.FamilyName}".ToLowerInvariant();
            request.Custom = request.ReplaceCustomValues(custom);

            return await _tools.IssueJwtAsync(3600, request.Claims);
        }
     }
}