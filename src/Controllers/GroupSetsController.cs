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
    /// Course Groups service: the group sets of a course.
    /// See https://www.imsglobal.org/spec/lti-gs/v1p0#group-sets-service.
    /// </summary>
    public class GroupSetsController : GroupSetsControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CourseAccessValidator _courseValidator;

        public GroupSetsController(
            IWebHostEnvironment env,
            ILogger<GroupSetsController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        protected override async Task<ActionResult<ContextGroupSetContainer>> OnGetGroupSetsAsync(GetGroupSetsRequest request)
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

            // A course has a handful of sets, so the optional limit (paging) is ignored.
            var groupSets = await _context.GetGroupSetsAsync(contextId);

            return new ContextGroupSetContainer
            {
                Id = Request.GetDisplayUrl(),
                Sets = groupSets
                    .Select(s => new GroupSet { Id = s.Id.ToString(), Name = s.Name, Tag = s.Tag })
                    .ToList()
            };
        }
    }
}
