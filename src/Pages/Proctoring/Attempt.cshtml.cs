using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.Proctoring;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Proctoring
{
    /// <summary>
    /// The assessment itself, which this platform delivers once the proctoring tool started
    /// the attempt. It is a pretend assessment: the point is to show the attempt's status
    /// and the Assessment Control actions the proctoring tool sends while it runs.
    /// </summary>
    public class AttemptModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AttemptModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ProctoringAttempt Attempt { get; private set; }

        public Person Candidate { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            return await LoadAsync(id) ? Page() : NotFound();
        }

        /// <summary>
        /// Polled by the page, so a pause or terminate from the proctoring tool shows up.
        /// </summary>
        public async Task<IActionResult> OnGetStatusAsync(int id)
        {
            if (!await LoadAsync(id)) return NotFound();

            return new JsonResult(new { status = Attempt.Status.ToString(), events = Attempt.Events.Count });
        }

        /// <summary>
        /// The candidate hands in the assessment.
        /// </summary>
        public async Task<IActionResult> OnPostFinishAsync(int id)
        {
            if (!await LoadAsync(id)) return NotFound();

            if (Attempt.Status != AssessmentControlStatus.Running)
            {
                return RedirectToPage(new { id });
            }

            Attempt.Status = AssessmentControlStatus.Complete;
            Attempt.EndedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // The tool asked to hear when the assessment ends (end_assessment_return), so
            // launch it with LtiEndAssessment.
            if (Attempt.EndAssessmentReturn)
            {
                var user = await _context.GetUserFullAsync(User);
                return RedirectToPage("/OidcLaunch", new
                {
                    id = Attempt.Id,
                    messageType = Constants.Lti.LtiEndAssessmentRequestMessageType,
                    courseId = user.Course.Id,
                    personId = Attempt.PersonId
                });
            }

            return RedirectToPage(new { id });
        }

        private async Task<bool> LoadAsync(int id)
        {
            var user = await _context.GetUserFullAsync(User);
            Attempt = await _context.GetProctoringAttemptAsync(id);
            if (user == null || Attempt == null || user.Course.ResourceLinks.All(l => l.Id != Attempt.ResourceLinkId))
            {
                return false;
            }

            Candidate = user.People.SingleOrDefault(p => p.Id == Attempt.PersonId);
            return true;
        }
    }
}
