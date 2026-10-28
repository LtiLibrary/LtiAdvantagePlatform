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
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            var groups = (await _context.GetGroupSetsAsync(course.Id, request.CancellationToken))
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
