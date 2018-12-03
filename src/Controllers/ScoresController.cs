using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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

        protected override async Task<IActionResult> OnPostScoreAsync(PostScoreRequest request)
        {
            var course = await _context.GetCourseByContextIdAsync(request.ContextId);
            if (course == null)
            {
                return NotFound();
            }

            var gradebookColumn = course.GradebookColumns.SingleOrDefault(c => c.Id == Convert.ToInt32(request.Id));
            if (gradebookColumn == null)
            {
                return NotFound();
            }

            // Save the score
            var scoreUrl = Request.GetDisplayUrl().EnsureTrailingSlash() + "1";
            return new ScoreResult(request.Score, StatusCodes.Status201Created);
        }
    }
}
