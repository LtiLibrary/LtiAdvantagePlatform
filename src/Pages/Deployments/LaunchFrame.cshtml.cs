using System.Globalization;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using LtiAdvantageLibrary.NetCore.Lti;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Deployments
{
    public class LaunchFrameModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;
        private readonly IdentityServerTools _tools;

        public LaunchFrameModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager, IdentityServerTools tools)
        {
            _context = context;
            _userManager = userManager;
            _tools = tools;
        }

        public string IdToken { get; private set; }
        public string ToolUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(string id, string persona)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var deployment = await _context.Deployments
                .Include(m => m.Tool)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (deployment == null)
            {
                return NotFound();
            }

            var person = persona == "teacher"
                ? await _context.People.FindAsync(user.TeacherId)
                : await _context.People.FindAsync(user.StudentId);

            var course = deployment.ToolPlacement == Deployment.ToolPlacements.Course
                ? await _context.Courses.FindAsync(user.CourseId)
                : null;

            var platform = await _context.Platforms.FindAsync(user.PlatformId);

            IdToken = await GetJwtAsync(deployment, person, course, platform);
            ToolUrl = deployment.Tool.Url;

            return Page();
        }

        private async Task<string> GetJwtAsync(Deployment deployment, Person person, Course course, Platform platform)
        {
            var request = new LtiResourceLinkRequest
            {
                MessageType = LtiConstants.LtiResourceLinkRequestMessageType,
                Version = LtiConstants.Version,
                DeploymentId = deployment.Id,
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Id = deployment.Id,
                    Title = deployment.Tool.Name
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

            request.Audiences = new [] { deployment.ClientId.ToString() };

            return await _tools.IssueJwtAsync(3600, request.Claims);
        }
     }
}