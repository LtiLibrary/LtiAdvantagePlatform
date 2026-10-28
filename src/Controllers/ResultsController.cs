using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.AspNetCore.AssignmentGradeServices;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
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