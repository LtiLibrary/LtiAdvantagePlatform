using System.Threading.Tasks;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class ResultsController : ResultsControllerBase
    {
        private readonly ILogger<ResultsControllerBase> _logger;

        public ResultsController(ILogger<ResultsControllerBase> logger) : base(logger)
        {
            _logger = logger;
        }

        protected override Task<ResultContainerResult> OnGetResultsAsync(GetResultsRequest request)
        {
            try
            {
                _logger.LogInformation($"Starting {nameof(OnGetResultsAsync)}.");

                // Since there are no scores, there are no results.
                return Task.FromResult(new ResultContainerResult(new ResultContainer()));
            }
            finally 
            {
                _logger.LogInformation($"Exiting {nameof(OnGetResultsAsync)}.");
            }
        }
    }
}