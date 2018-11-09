using System.Globalization;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
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

        private async Task<string> GetJwtAsync(ResourceLink resourceLink, Tool tool, Client client, Person person, Course course, Platform platform)
        {
            var request = new LtiResourceLinkRequest
            {
                MessageType = LtiConstants.LtiResourceLinkRequestMessageType,
                Version = LtiConstants.Version,
                DeploymentId = tool.DeploymentId,
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Id = resourceLink.Id.ToString(),
                    Title = resourceLink.Title
                },
                GivenName = person.FirstName,
                FamilyName = person.LastName,
                Locale = CultureInfo.CurrentUICulture.Name,
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.iframe,
                    ReturnUrl = Request.GetDisplayUrl()
                }
            };

            if (course != null)
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id,
                    Title = course.Name,
                    Type = new[] {ContextType.CourseSection}
                };
                request.Roles = person.IsStudent
                    ? new[] {Role.ContextLearner, Role.InstitutionStudent}
                    : new[] {Role.ContextInstructor, Role.InstitutionFaculty};
            }
            else
            {
                request.Roles = person.IsStudent
                    ? new[] {Role.InstitutionLearner}
                    : new[] {Role.InstitutionFaculty};
            }

            request.UserId = person.Id;
            request.Platform = new PlatformClaimValueType
            {
                ContactEmail = platform.ContactEmail,
                Description = platform.Description,
                Guid = platform.Id,
                Name = platform.Name,
                ProductFamilyCode = "LtiAdvantageLibrary",
                Url = Request.GetDisplayUrl(),
                Version = "1.0"
            };

            request.Nonce = LtiResourceLinkRequest.GenerateCryptographicNonce();

            request.Audiences = new [] { client.ClientId };

            return await _tools.IssueJwtAsync(3600, request.Claims);
        }
     }
}