using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Http.Extensions;
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
        /// Returns the maximum score for each person with scores.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override async Task<ResultContainerResult> OnGetResultsAsync(GetResultsRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return ResultsNotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return ResultsNotFound();
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
                    ScoreOf = Url.Link(Constants.ServiceEndpoints.AgsLineItemService, new { request.ContextId, request.Id }),
                    UserId = g.Key
                });

            var resultContainer = new ResultContainer();
            foreach (var result in results)
            {
                resultContainer.Add(result);
            }

            return ResultsOk(resultContainer);
        }
    }
}