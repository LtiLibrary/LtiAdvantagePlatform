using System.Globalization;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantageLibrary.NetCore;
using LtiAdvantageLibrary.NetCore.Lti;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchFrameModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly IdentityServerTools _tools;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public LaunchFrameModel(
            ApplicationDbContext appContext, 
            IConfigurationDbContext identityContext,
            IdentityServerTools tools,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _tools = tools;
            _userManager = userManager;
        }

        public string IdToken { get; private set; }
        public string ToolUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? id, string persona)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var resourceLink = await _appContext.ResourceLinks
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            var tool = await _appContext.Tools.FindAsync(resourceLink.ToolId);
            if (tool == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);
            if (client == null)
            {
                return NotFound();
            }

            var person = persona == "teacher"
                ? await _appContext.People.FindAsync(user.TeacherId)
                : await _appContext.People.FindAsync(user.StudentId);

            var course = resourceLink.LinkContext == ResourceLink.LinkContexts.Course
                ? await _appContext.Courses.FindAsync(user.CourseId)
                : null;

            var platform = await _appContext.Platforms.FindAsync(user.PlatformId);

            IdToken = await GetJwtAsync(resourceLink, tool, client, person, course, platform);
            ToolUrl = tool.Url;

            return Page();
        }

        private async Task<string> GetJwtAsync(ResourceLink resourceLink, 
            Tool tool, Client client, Person person, Course course, Platform platform)
        {
            var request = new LtiResourceLinkRequest
            {
                Audiences = new [] { client.ClientId },
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.iframe,
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
                    ProductFamilyCode = "LtiAdvantageLibrary",
                    Url = Request.GetDisplayUrl(),
                    Version = "1.0"
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
                request.Roles = person.IsStudent
                    ? new[] {Role.ContextLearner, Role.InstitutionStudent}
                    : new[] {Role.ContextInstructor, Role.InstitutionFaculty};

                // Only include Names and Role Provisioning Service claim if
                // the launch includes a context.
                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    ContextMembershipUrl = 
                        Url.RouteUrl(Constants.LtiClaimNames.NamesRoleService, 
                            new { contextId = course.Id },
                            "https",
                            Request.Host.ToString())
                };
            }
            else
            {
                request.Roles = person.IsStudent
                    ? new[] {Role.InstitutionLearner}
                    : new[] {Role.InstitutionFaculty};
            }

            return await _tools.IssueJwtAsync(3600, request.Claims);
        }
     }
}