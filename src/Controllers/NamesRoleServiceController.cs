using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.Lti;
using LtiAdvantageLibrary.NamesRoleService;
using Microsoft.AspNetCore.Http.Extensions;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample membership controller that implements the Membership service.
    /// See https://www.imsglobal.org/spec/lti-nrps/v2p0.
    /// </summary>
    public class NamesRoleServiceController : NamesRoleServiceControllerBase
    {
        private readonly ApplicationDbContext _appContext;

        public NamesRoleServiceController(ApplicationDbContext appContext)
        {
            _appContext = appContext;
        }

        /// <summary>
        /// Sample implementation of OnGetMembershipAsync returns both members of the
        /// sample course. This sample ignores limit, rlid, and role parameters.
        /// </summary>
        /// <param name="request">The <see cref="GetNamesRolesRequest"/> including the course id.</param>
        /// <returns>The members of the sample course.</returns>
        protected override async Task<GetNamesRolesResponse> OnGetMembershipAsync(GetNamesRolesRequest request)
        {
            var response = new GetNamesRolesResponse
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

            var user = await _appContext.GetUserAsync(course.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var people = user.People;
            if (people.Any())
            {
                response.MembershipContainer.Members = people
                    .Select(p => new Member
                    {
                        ContextId = course.Id,
                        ContextTitle = course.Name,
                        FamilyName = p.LastName,
                        GivenName = p.FirstName,
                        Roles = AdvantagePlatform.Areas.Identity.Pages.Account.Manage.PeopleModel.ParsePersonRoles(p.Roles),
                        Status = MemberStatus.Active,
                        SourcedId = p.SisId,
                        UserId = p.Id
                    })
                    .ToList();
            }

            return response;
        }
    }
}
