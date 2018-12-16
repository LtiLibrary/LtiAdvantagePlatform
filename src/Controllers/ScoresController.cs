using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Sample scores controller.
    /// </summary>
    public class ScoresController : ScoresControllerBase
    {
        private readonly ApplicationDbContext _context;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScoresController(
            ApplicationDbContext context,
            ILogger<ScoresControllerBase> logger) : base(logger)
        {
            _context = context;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add a score to a line item.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>The score that was added.</returns>
        protected override async Task<ActionResult<Score>> OnAddScoreAsync(AddScoreRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.ContextId)} not found."});
            }

            if (!int.TryParse(request.LineItemId, out var id))
            {
                return BadRequest($"{nameof(request.LineItemId)} is not a valid line item id.");
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == id);
            if (gradebookColumn == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.LineItemId)} not found."});
            }

            if (!int.TryParse(request.Score.UserId, out var personId))
            {
                return BadRequest($"{nameof(request.Score.UserId)} is not a valid user id.");
            }

            var person = await _context.People.FindAsync(personId);
            if (person == null)
            {
                return NotFound(new ProblemDetails {Title = $"{nameof(request.Score.UserId)} not found."});
            }

            var gradebookRow = new GradebookRow
            {
                ActivityProgress = request.Score.ActivityProgress,
                Comment = request.Score.Comment,
                GradingProgress = request.Score.GradingProgress,
                PersonId = request.Score.UserId,
                ScoreGiven = request.Score.ScoreGiven,
                ScoreMaximum = request.Score.ScoreMaximum,
                TimeStamp = request.Score.TimeStamp
            };

            gradebookColumn.Scores.Add(gradebookRow);
            await _context.SaveChangesAsync();

            var url = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.AgsScoreService,
                new {request.ContextId, lineItemId = request.LineItemId, gradebookRow.Id});

            // Save the score
            return Created(url, request.Score);
        }
    }
}
