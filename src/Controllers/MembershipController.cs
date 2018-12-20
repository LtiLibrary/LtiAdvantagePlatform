using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using LtiAdvantage.IdentityServer4;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
            IHostingEnvironment env,
            ILogger<MembershipController> logger,
            ApplicationDbContext context) : base(env, logger)
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
            // In this sample app, each registered app user has an associated platform,
            // course, and membership. So look up the user that owns the requested course.

            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
                        
            var user = await _context.GetUserAsync(User);
            if (user.Course.Id != contextId)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "You are not authorized to access the requested course."
                });
            }

            var course = await _context.GetCourseAsync(contextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title= ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

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
                        UserId = p.Id.ToString()
                    });

                if (request.Rlid.IsPresent())
                {
                    if (!int.TryParse(request.Rlid, out var resourceLinkId))
                    {
                        var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                        ModelState.AddModelError(name, $"The {name} field cannot be converted into a resource linkid id.");
                        return BadRequest(new ValidationProblemDetails(ModelState));
                    }

                    people = people.Where(p => user.Course.ResourceLinks.Any(l => l.Id == resourceLinkId));
                }

                if (request.Role.HasValue)
                {
                    people = people.Where(p => p.Roles.Any(r => r == request.Role.Value));
                }

                membership.Members = people.ToList();
            }

            return membership;
        }
    }
}
