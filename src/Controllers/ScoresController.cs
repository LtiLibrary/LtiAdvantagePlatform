using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
            IHostingEnvironment env,
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

            if (!int.TryParse(request.Score.UserId, out var personId))
            {
                var name = $"{nameof(request)}.{nameof(request.Score)}.{nameof(request.Score.UserId)}";
                ModelState.AddModelError(name, $"The {name} field cannot be converted into a user id.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
                        
            if (!await _courseValidator.UserHasAccess(contextId))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "User not authorized to access the requested course."
                });
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

            var person = await _context.GetPersonAsync(personId);
            if (person == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound), 
                    Detail = "Person not found"
                });
            }

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
            await _context.SaveChangesAsync();

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
                        
            var user = await _context.GetUserLightAsync(User);
            if (user.Course.Id != contextId)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Not authorized",
                    Detail = "You are not authorized to access the requested course."
                });
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
                UserId = gradebookRow.PersonId.ToString()
            };
        }
    }
}
