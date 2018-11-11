using System.Collections.Generic;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.Lti;
using LtiAdvantageLibrary.Membership;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample membership controller that implements the Membership service.
    /// See https://www.imsglobal.org/spec/lti-nrps/v2p0.
    /// </summary>
    public class MembershipController : MembershipControllerBase
    {
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public MembershipController(ApplicationDbContext appContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        /// <summary>
        /// Sample implementation of OnGetMembershipAsync returns both members of the
        /// sample course. This sample ignores limit, rlid, and role parameters.
        /// </summary>
        /// <param name="request">The <see cref="GetMembershipRequest"/> including the course id.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<GetMembershipResponse> OnGetMembershipAsync(GetMembershipRequest request)
        {
            var response = new GetMembershipResponse
            {
                MembershipContainer = new MembershipContainer
                {
                    Id = Request.GetDisplayUrl()
                }
            };

            var course = await _appContext.Courses.FindAsync(request.ContextId);
            if (course == null)
            {
                return NotFound("Context not found");
            }

            var user = await _userManager.FindByIdAsync(course.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var student = await _appContext.People.FindAsync(user.StudentId);
            var teacher = await _appContext.People.FindAsync(user.TeacherId);

            response.MembershipContainer.Members = new List<Member>
            {
                new Member
                {
                    ContextId = course.Id,
                    ContextTitle = course.Name,
                    FamilyName = student.LastName,
                    GivenName = student.FirstName,
                    Roles = new [] { Role.ContextLearner, Role.InstitutionLearner },
                    Status = MemberStatus.Active,
                    SourcedId = student.SisId,
                    UserId = student.Id
                },
                new Member
                {
                    ContextId = course.Id,
                    ContextTitle = course.Name,
                    FamilyName = teacher.LastName,
                    GivenName = teacher.FirstName,
                    Roles = new [] { Role.ContextInstructor, Role.InstitutionStaff },
                    SourcedId = teacher.SisId,
                    Status = MemberStatus.Active,
                    UserId = teacher.Id
                }
            };

            return response;
        }
    }
}
