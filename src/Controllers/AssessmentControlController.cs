using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.AspNetCore.Proctoring;
using LtiAdvantage.Proctoring;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AdvantagePlatform.Controllers
{
    /// <summary>
    /// The Assessment Control service of Proctoring Services 1.0 (§5): while an attempt runs,
    /// the proctoring tool can pause, resume or terminate it, grant extra time (update), or
    /// record an incident (flag). The library checks the request; this controller finds the
    /// attempt, applies the action and records it for the platform UI.
    /// </summary>
    public class AssessmentControlController : AssessmentControlControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AssessmentControlController> _logger;

        public AssessmentControlController(
            IWebHostEnvironment env,
            ILogger<AssessmentControlController> logger,
            ApplicationDbContext context) : base(env, logger)
        {
            _context = context;
            _logger = logger;
        }

        protected override async Task<ActionResult<AssessmentControlResponse>> OnControlAssessmentAsync(ControlAssessmentRequest request)
        {
            var control = request.Control;

            // user.iss is the issuer that identified the candidate: this platform.
            if (control.User.Iss.TrimEnd('/') != Request.GetIssuer().TrimEnd('/'))
            {
                return ServiceProblems.BadRequest($"user.iss must be {Request.GetIssuer()}.");
            }

            // Only the tool that proctors the assessment may control its attempts. Its client
            // id is in the access token.
            var clientId = User.FindFirst(OpenIddictConstants.Claims.ClientId)?.Value
                           ?? User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;

            // user.sub is the candidate's "sub" from the launch: the person id.
            var attempt = await _context.ProctoringAttempts
                .Include(a => a.ResourceLink)
                .ThenInclude(l => l.Tool)
                .SingleOrDefaultAsync(a =>
                    a.ResourceLink.Id.ToString() == control.ResourceLink.Id &&
                    a.PersonId.ToString() == control.User.Sub &&
                    a.AttemptNumber == control.AttemptNumber &&
                    a.ResourceLink.Tool.ClientId == clientId,
                    request.CancellationToken);

            if (attempt == null)
            {
                // Also when the attempt belongs to another tool's assessment: don't reveal it exists.
                return ServiceProblems.NotFound("No attempt of this tool's assessment matches resource_link, user and attempt_number.");
            }

            switch (control.Action)
            {
                case AssessmentControlAction.Pause when attempt.Status == AssessmentControlStatus.Running:
                    attempt.Status = AssessmentControlStatus.Paused;
                    break;
                case AssessmentControlAction.Resume when attempt.Status == AssessmentControlStatus.Paused:
                    attempt.Status = AssessmentControlStatus.Running;
                    break;
                case AssessmentControlAction.Terminate when attempt.Status is AssessmentControlStatus.Running or AssessmentControlStatus.Paused:
                    attempt.Status = AssessmentControlStatus.Terminated;
                    attempt.EndedAt = DateTime.UtcNow;
                    break;
                case AssessmentControlAction.Update:
                    attempt.ExtraTime += control.ExtraTime ?? 0;
                    break;
                // Flag only records the incident. Other actions that don't fit the current
                // status (resume while running, ...) leave it as it is; the response says so.
            }

            attempt.Events.Add(new ProctoringEvent
            {
                Action = control.Action,
                ReceivedAt = DateTime.UtcNow,
                IncidentTime = control.IncidentTime,
                IncidentSeverity = control.IncidentSeverity,
                ExtraTime = control.ExtraTime,
                ReasonCode = control.ReasonCode,
                ReasonMessage = control.ReasonMsg,
                ResultingStatus = attempt.Status
            });
            await _context.SaveChangesAsync(request.CancellationToken);

            _logger.LogInformation("Assessment control {Action} on attempt {AttemptId}: now {Status}.",
                control.Action, attempt.Id, attempt.Status);

            // extra_time is the total granted so far, not just this request's.
            return new AssessmentControlResponse
            {
                Status = attempt.Status,
                ExtraTime = attempt.ExtraTime
            };
        }
    }
}
