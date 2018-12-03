using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
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
                return new ResultContainerResult(StatusCodes.Status404NotFound);
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return new ResultContainerResult(StatusCodes.Status404NotFound);
            }

            var results = gradebookColumn.Scores
                .OrderBy(s => s.TimeStamp)
                .GroupBy(s => s.PersonId)
                .Select(g => new Result
                {
                    Id = Request.GetDisplayUrl().EnsureTrailingSlash() + g.Key,
                    ResultMaximum = g.Max(x => x.ScoreMaximum),
                    ResultScore = g.Max(x => x.ScoreGiven),
                    ScoreOf = Url.Link(Constants.ServiceEndpoints.AgsLineItemService, new { request.ContextId, request.Id }),
                    UserId = g.Key
                });
            var resultContainer = new ResultContainer();
            foreach (var result in results)
            {
                resultContainer.Add(result);
            }

            return new ResultContainerResult(resultContainer);
        }
    }
}