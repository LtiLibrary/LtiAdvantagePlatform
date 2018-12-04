using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    public class ScoresController : ScoresControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ScoresController(
            ApplicationDbContext context,
            ILogger<ScoresControllerBase> logger) : base(logger)
        {
            _context = context;
        }

        protected override async Task<ScoreResult> OnPostScoreAsync(PostScoreRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return ScoreNotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return ScoreNotFound();
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

            // Save the score
            return ScoreCreated(request.Score);
        }
    }
}
