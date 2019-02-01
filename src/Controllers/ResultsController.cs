using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
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
    /// Sample results controller that returns highest score for each lineitem.
    /// </summary>
    public class ResultsController : ResultsControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CourseAccessValidator _courseValidator;

        public ResultsController(
            IHostingEnvironment env,
            ILogger<ResultsController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns the most recent score for each person with scores.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<ActionResult<ResultContainer>> OnGetResultsAsync(GetResultsRequest request)
        {
            if (!int.TryParse(request.ContextId, out var contextId))
            {
                var name = $"{nameof(request)}.{nameof(request.ContextId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a course id.");
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                var name = $"{nameof(request)}.{nameof(request.LineItemId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a gradebook column id.");
            }

            if (!ModelState.IsValid)
            {
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
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Course not found"
                });
            }

            if (course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return NotFound(new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Gradebook column not found"
                });
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId);

            var results = gradebookColumn.Scores
                .OrderBy(s => s.TimeStamp)
                .GroupBy(s => s.PersonId)
                .Select(g => new Result
                {
                    Id = Request.GetDisplayUrl().EnsureTrailingSlash() + g.Key,
                    Comment = $"Last score of {g.Count()} attempt/s." 
                              + $"<p><div>First Score: {g.First().ScoreGiven:N1}</div>" 
                              + $"<div>Highest Score: {g.Max(x => x.ScoreGiven):N1}</div>" 
                              + $"<div>Lowest Score: {g.Min(x => x.ScoreGiven):N1}</div></p>",
                    ResultMaximum = g.Max(x => x.ScoreMaximum),
                    ResultScore = g.Last().ScoreGiven,
                    ScoreOf = Url.Link(Constants.ServiceEndpoints.Ags.LineItemService, 
                        new { contextId = request.ContextId, lineItemId = request.LineItemId }),
                    UserId = g.Key.ToString()
                })
                .ToList();

            if (request.UserId.IsPresent())
            {
                results = results.Where(r => r.UserId == request.UserId).ToList();
            }

            return new ResultContainer(results);
        }
    }
}