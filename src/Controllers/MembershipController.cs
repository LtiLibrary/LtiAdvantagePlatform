using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AspNetCore.NamesRoleProvisioningService;
using LtiAdvantage.CourseGroupsService;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample membership controller that implements the Membership service.
    /// See https://www.imsglobal.org/spec/lti-nrps/v2p0.
    /// </summary>
    public class MembershipController : MembershipControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CourseAccessValidator _courseValidator;

        public MembershipController(
            IWebHostEnvironment env,
            ILogger<MembershipController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns members of the course.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<ActionResult<MembershipContainer>> OnGetMembershipAsync(GetMembershipRequest request)
        {
            // In this sample app, each registered app user has an associated platform,
            // course, and membership. So look up the user that owns the requested course.

            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            var user = await _context.GetUserByCourseIdAsync(course.Id, request.CancellationToken);

            var membership = new MembershipContainer
            {
                Id = Request.GetDisplayUrl(),
                Context = new Context
                {
                    Id = user.Course.Id.ToString(),
                    Title = user.Course.Name
                }
            };

            if (user.People.Any())
            {
                var people = user.People
                    .Select(p => new Member
                    {
                        FamilyName = p.LastName,
                        GivenName = p.FirstName,
                        Roles = PeopleModel.ParsePersonRoles(p.Roles),
                        Status = MemberStatus.Active,
                        LisPersonSourcedId = p.SisId,
                        UserId = p.Id.ToString(),
                        Email = p.Email
                    });

                if (request.Rlid.IsPresent())
                {
                    // Every member of the sample course can use every link in it, so the filter
                    // only checks that the link is in this course.
                    if (user.Course.ResourceLinks.All(l => l.Id.ToString() != request.Rlid))
                    {
                        return ServiceProblems.NotFound($"Resource link {request.Rlid} not found in this course.");
                    }
                }

                if (request.Role.HasValue)
                {
                    people = people.Where(p => p.Roles.Any(r => r == request.Role.Value));
                }

                // ?groups=true (Course Groups service): each member's group enrollments.
                // The base class only asks for them when the token has the groups scope.
                if (request.IncludeGroups)
                {
                    var groups = (await _context.GetGroupSetsAsync(course.Id, request.CancellationToken)).SelectMany(s => s.Groups).ToList();
                    people = people.Select(member =>
                    {
                        member.GroupEnrollments = groups
                            .Where(g => g.Members.Any(m => m.Id.ToString() == member.UserId))
                            .Select(g => new GroupEnrollment { GroupId = g.Id.ToString() })
                            .ToArray();
                        return member;
                    }).ToList();
                }

                membership.Members = ServicePaging.Page(HttpContext, people, request.Limit);
            }

            return membership;
        }
    }
}
