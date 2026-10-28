using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.AspNetCore.AssignmentGradeServices;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            IWebHostEnvironment env,
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
            var (course, problem) = await _courseValidator.GetCourseAsync(request.ContextId, request.CancellationToken);
            if (problem != null)
            {
                return problem;
            }

            if (!int.TryParse(request.LineItemId, out var lineItemId) || course.GradebookColumns.All(c => c.Id != lineItemId))
            {
                return ServiceProblems.NotFound($"Line item {request.LineItemId} not found.");
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId, request.CancellationToken);

            var results = gradebookColumn.Scores
                .OrderBy(s => s.TimeStamp)
                .GroupBy(s => s.PersonId)
                .Select(g => new Result
                {
                    // The result's own URL: the results URL without any query string.
                    Id = Url.Link(Constants.ServiceEndpoints.Ags.ResultsService,
                        new { contextId = request.ContextId, lineItemId = request.LineItemId }).EnsureTrailingSlash() + g.Key,
                    // The comment the tool sent with the most recent score (AGS §3.3.5).
                    Comment = g.Last().Comment,
                    // A progress-only score has no scoreGiven, so the result's score is the last
                    // one the tool actually gave. resultMaximum defaults to 1 when nothing says otherwise.
                    ResultMaximum = g.LastOrDefault(x => x.ScoreGiven.HasValue)?.ScoreMaximum
                                    ?? gradebookColumn.ScoreMaximum ?? 1,
                    ResultScore = g.LastOrDefault(x => x.ScoreGiven.HasValue)?.ScoreGiven,
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