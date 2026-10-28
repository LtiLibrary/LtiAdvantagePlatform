using System;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.Lti;
using LtiAdvantage.Proctoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Pages.Proctoring
{
    /// <summary>
    /// The start_assessment_url of Proctoring Services 1.0 (§4.3). After checking the candidate,
    /// the proctoring tool form-posts a signed LtiStartAssessment message here (as "JWT").
    /// The platform matches it to the attempt by session_data, starts the attempt and shows
    /// the assessment.
    /// </summary>
    [AllowAnonymous] // The signed message is the proof, like a deep linking response.
    [IgnoreAntiforgeryToken]
    public class StartAssessmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolMessageValidator _toolMessageValidator;
        private readonly ILogger<StartAssessmentModel> _logger;

        public StartAssessmentModel(
            ApplicationDbContext context,
            ToolMessageValidator toolMessageValidator,
            ILogger<StartAssessmentModel> logger)
        {
            _context = context;
            _toolMessageValidator = toolMessageValidator;
            _logger = logger;
        }

        [BindProperty(Name = "JWT")]
        public string Jwt { get; set; }

        public string Error { get; private set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var (token, tool, error) = await _toolMessageValidator.ValidateAsync(
                Jwt, Constants.Lti.LtiStartAssessmentRequestMessageType, Request);
            if (token == null)
            {
                return Reject(error);
            }

            var message = new LtiStartAssessmentRequest(token.Payload);

            // session_data ties this message to the LtiStartProctoring the platform sent.
            var attempt = await _context.ProctoringAttempts
                .Include(a => a.ResourceLink)
                .ThenInclude(l => l.Tool)
                .SingleOrDefaultAsync(a => a.SessionData == message.SessionData);
            if (attempt == null
                || attempt.ResourceLink.Tool.Id != tool.Id
                || attempt.ResourceLink.Id.ToString() != message.ResourceLink?.Id
                || attempt.AttemptNumber != message.AttemptNumber)
            {
                return Reject("The message does not match a proctoring attempt (session_data, resource_link or attempt_number).");
            }

            if (attempt.Status != AssessmentControlStatus.None)
            {
                // The session_data can start its attempt only once.
                return Reject($"Attempt {attempt.AttemptNumber} has already started.");
            }

            attempt.Status = AssessmentControlStatus.Running;
            attempt.StartedAt = DateTime.UtcNow;
            attempt.EndAssessmentReturn = message.EndAssessmentReturn == true;
            attempt.VerifiedUserName = message.VerifiedUser?.Name
                ?? $"{message.VerifiedUser?.GivenName} {message.VerifiedUser?.FamilyName}".Trim();

            // The tool may ask the platform to keep new proctoring settings for later messages.
            if (message.ProctoringSettings?.Data != null)
            {
                attempt.ResourceLink.ProctoringSettings = message.ProctoringSettings.Data;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Proctoring attempt {AttemptId} started by {Tool}.", attempt.Id, tool.Name);

            return RedirectToPage("./Attempt", new { id = attempt.Id });
        }

        private IActionResult Reject(string error)
        {
            _logger.LogError("LtiStartAssessment rejected: {Error}", error);
            Error = error;
            Response.StatusCode = 400;
            return Page();
        }
    }
}
