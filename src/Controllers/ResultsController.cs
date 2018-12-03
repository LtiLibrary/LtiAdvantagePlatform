using System.Threading.Tasks;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class ResultsController : ResultsControllerBase
    {
        public ResultsController(ILogger<ResultsControllerBase> logger) : base(logger)
        {
        }

        protected override Task<ResultContainerResult> OnGetResultsAsync(GetResultsRequest request)
        {
            // Since there are no scores, there are no results.
            return Task.FromResult(new ResultContainerResult(new ResultContainer()));
        }
    }
}