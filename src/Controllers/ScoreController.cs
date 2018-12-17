using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample score controller.
    /// </summary>
    public class ScoreController : ScoreControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScoreController(
            ApplicationDbContext context,
            ILogger<ScoreControllerBase> logger) : base(logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a score.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>The score.</returns>
        protected override async Task<ActionResult<Score>> OnGetScoreAsync(GetScoreRequest request)
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

            if (!int.TryParse(request.ScoreId, out var scoreId))
            {
                var name = $"{nameof(request)}.{nameof(request.ScoreId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a score id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
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

            if (gradebookColumn.Scores.All(s => s.Id != scoreId))
            {
                return NotFound(new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Score not found"
                });
            }

            var gradebookRow = await _context.GetGradebookRowAsync(scoreId);

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
