using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AspNetCore.CourseGroupsService;
using LtiAdvantage.CourseGroupsService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <summary>
    /// Course Groups service: the groups of a course, optionally only those of one user.
    /// See https://www.imsglobal.org/spec/lti-gs/v1p0#groups-service.
    /// </summary>
    public class GroupsController : GroupsControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CourseAccessValidator _courseValidator;

        public GroupsController(
            IWebHostEnvironment env,
            ILogger<GroupsController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        protected override async Task<ActionResult<ContextGroupContainer>> OnGetGroupsAsync(GetGroupsRequest request)
        {
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Course not found" });
            }

            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
            }

            var groups = (await _context.GetGroupSetsAsync(contextId))
                .SelectMany(s => s.Groups)
                .OrderBy(g => g.Name)
                .AsEnumerable();

            // ?user_id= asks for the groups that user is a member of. The user id is the
            // same one launches and the membership service use: the person id.
            if (request.UserId.IsPresent())
            {
                groups = groups.Where(g => g.Members.Any(m => m.Id.ToString() == request.UserId));
            }

            return new ContextGroupContainer
            {
                Id = Request.GetDisplayUrl(),
                UserId = request.UserId,
                Groups = groups
                    .Select(g => new Group
                    {
                        Id = g.Id.ToString(),
                        Name = g.Name,
                        Tag = g.Tag,
                        SetIds = new[] { g.GroupSetId.ToString() }
                    })
                    .ToList()
            };
        }
    }
}
