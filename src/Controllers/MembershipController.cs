using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public MembershipController(
            ILogger<MembershipControllerBase> logger, 
            ApplicationDbContext context) : base(logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns members of the course. Ignores filters.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<ActionResult<MembershipContainer>> OnGetMembershipAsync(GetMembershipRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.People)
                .SingleOrDefaultAsync(u => u.Course == course);
            if (user == null)
            {
                return NotFound();
            }

            var membership = new MembershipContainer
            {
                Id = Request.GetDisplayUrl(),
                Context = new Context
                {
                    Id = course.Id.ToString(),
                    Title = course.Name
                }
            };

            if (user.People.Any())
            {
                membership.Members = user.People
                    .Select(p => new Member
                    {
                        FamilyName = p.LastName,
                        GivenName = p.FirstName,
                        Roles = PeopleModel.ParsePersonRoles(p.Roles),
                        Status = MemberStatus.Active,
                        LisPersonSourcedId = p.SisId,
                        UserId = p.Id.ToString()
                    })
                    .ToList();
            }

            return membership;
        }
    }
}
