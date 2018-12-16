using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
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

        public ResultsController(
            ApplicationDbContext context,
            ILogger<ResultsControllerBase> logger) : base(logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns the most recent score for each person with scores.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<ActionResult<ResultContainer>> OnGetResultsAsync(GetResultsRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.ContextId)} not found."});
            }
                        
            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                return BadRequest($"{nameof(request.LineItemId)} is not a valid line item id.");
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == lineItemId);
            if (gradebookColumn == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.LineItemId)} not found."});
            }

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
                    ScoreOf = Url.Link(Constants.ServiceEndpoints.AgsLineItemService, new { request.ContextId, Id = request.LineItemId }),
                    UserId = g.Key
                });

            if (request.UserId.IsPresent())
            {
                results = results.Where(r => r.UserId == request.UserId);
            }

            var resultContainer = new ResultContainer();
            foreach (var result in results)
            {
                resultContainer.Add(result);
            }

            return resultContainer;
        }
    }
}