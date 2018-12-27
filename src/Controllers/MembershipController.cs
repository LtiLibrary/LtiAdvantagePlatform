﻿using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
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
        private readonly CourseAccessValidator _courseValidator;

        public MembershipController(
            IHostingEnvironment env,
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

            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
                        
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
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

            var user = await _context.GetUserByCourseIdAsync(course.Id);

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
