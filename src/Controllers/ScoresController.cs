using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AspNetCore.AssignmentGradeServices;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Hosting;
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
        private readonly CourseAccessValidator _courseValidator;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ScoresController(
            IWebHostEnvironment env,
            ILogger<ScoresController> logger,
            ApplicationDbContext context,
            CourseAccessValidator courseValidator) : base(env, logger)
        {
            _context = context;
            _courseValidator = courseValidator;
        }

        /// <inheritdoc />
        /// <summary>
        /// Add a score to a line item.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>The score that was added.</returns>
        protected override async Task<ActionResult<Score>> OnAddScoreAsync(AddScoreRequest request)
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

            // The score's userId is the "sub" of the launch: a person id.
            if (!int.TryParse(request.Score.UserId, out var personId)
                || await _context.GetPersonAsync(personId, request.CancellationToken) == null)
            {
                return ServiceProblems.BadRequest($"userId {request.Score.UserId} is not a member of the course.");
            }

            var gradebookColumn = await _context.GetGradebookColumnAsync(lineItemId, request.CancellationToken);

            var gradebookRow = new GradebookRow
            {
                ActivityProgress = request.Score.ActivityProgress,
                Comment = request.Score.Comment,
                GradingProgress = request.Score.GradingProgress,
                PersonId = personId,
                ScoreGiven = request.Score.ScoreGiven,
                ScoreMaximum = request.Score.ScoreMaximum,
                TimeStamp = request.Score.TimeStamp
            };

            gradebookColumn.Scores.Add(gradebookRow);
            await _context.SaveChangesAsync(request.CancellationToken);

            var url = Url.Link(LtiAdvantage.Constants.ServiceEndpoints.Ags.ScoreService,
                new {request.ContextId, lineItemId = request.LineItemId, scoreId = gradebookRow.Id});

            // Save the score
            return Created(url, request.Score);
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a score.
        /// </summary>
        /// <remarks>
        /// This is not part of the Assignment and Grade Services spec.
        /// </remarks>
        /// <param name="request">The request parameters.</param>
        /// <returns>The score.</returns>
        protected override async Task<ActionResult<Score>> OnGetScoreAsync(GetScoreRequest request)
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
            if (!int.TryParse(request.ScoreId, out var scoreId) || gradebookColumn.Scores.All(s => s.Id != scoreId))
            {
                return ServiceProblems.NotFound($"Score {request.ScoreId} not found.");
            }

            var gradebookRow = await _context.GetGradebookRowAsync(scoreId, request.CancellationToken);

            return new Score
            {
                ActivityProgress = gradebookRow.ActivityProgress,
                Comment = gradebookRow.Comment,
                GradingProgress = gradebookRow.GradingProgress,
                ScoreGiven = gradebookRow.ScoreGiven,
                ScoreMaximum = gradebookRow.ScoreMaximum,
                TimeStamp = gradebookRow.TimeStamp,
                UserId = gradebookRow.PersonId.ToString()
            };
        }
    }
}
