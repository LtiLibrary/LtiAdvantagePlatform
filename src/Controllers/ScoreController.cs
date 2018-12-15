using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class ScoreController : ScoreControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScoreController(
            ApplicationDbContext context,
            ILogger<ScoreControllerBase> logger) : base(logger)
        {
            _context = context;
        }

        protected override async Task<ActionResult<Score>> OnGetScoreAsync(GetScoreRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.ContextId)} not found."});
            }
            
            if (!int.TryParse(request.LineItemId, out var lineItemId))
            {
                return BadRequest($"{nameof(request.LineItemId)} is not an integer.");
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == lineItemId);
            if (gradebookColumn == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.LineItemId)} not found."});
            }
            
            if (!int.TryParse(request.Id, out var scoreId))
            {
                return BadRequest($"{nameof(request.Id)} is not an integer.");
            }

            var gradebookRow = gradebookColumn.Scores.SingleOrDefault(c => c.Id == scoreId);
            if (gradebookRow == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.Id)} not found."});
            }

            return new Score
            {
                ActivityProgress = gradebookRow.ActivityProgress,
                Comment = gradebookRow.Comment,
                GradingProgress = gradebookRow.GradingProgress,
                ScoreGiven = gradebookRow.ScoreGiven,
                ScoreMaximum = gradebookRow.ScoreMaximum,
                TimeStamp = gradebookRow.TimeStamp,
                UserId = gradebookRow.PersonId
            };
        }
    }
}
