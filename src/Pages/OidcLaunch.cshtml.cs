using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Pages
{
    public class OidcLaunchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OidcLaunchModel> _logger;

        public OidcLaunchModel(
            ApplicationDbContext context,
            ILogger<OidcLaunchModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initiates an LTI 1.3 launch from the platform to the tool by redirecting
        /// to the tool's OIDC login initiation URL.
        /// </summary>
        /// <param name="id">The resource link id, tool id (deep linking) or gradebook column id (submission review).</param>
        /// <param name="messageType">The lti message type.</param>
        /// <param name="courseId">The course id (or null if not launched from a course).</param>
        /// <param name="personId">The person id to impersonate.</param>
        /// <param name="forUserId">Submission review only: the person whose submission to show.</param>
        /// <remarks>For LtiStartProctoring, id is the proctored resource link; for LtiEndAssessment, the attempt.</remarks>
        public async Task<IActionResult> OnGetAsync(int id, string messageType, string courseId, string personId, string forUserId = null)
        {
            Tool tool;
            string targetLinkUri;
            int? attemptId = null;

            switch (messageType)
            {
                case Constants.Lti.LtiResourceLinkRequestMessageType:
                {
                    var resourceLink = await _context.GetResourceLinkAsync(id);
                    tool = resourceLink?.Tool;
                    targetLinkUri = tool?.LaunchUrl;
                    break;
                }
                case Constants.Lti.LtiDeepLinkingRequestMessageType:
                {
                    tool = await _context.GetToolAsync(id);
                    targetLinkUri = tool?.DeepLinkingLaunchUrl.IfMissingThen(tool.LaunchUrl);
                    break;
                }
                case Constants.Lti.LtiSubmissionReviewRequestMessageType:
                {
                    // Launched from a gradebook cell. The line item's submissionReview.url, if
                    // any, replaces the resource link's launch URL (Submission Review §2.2).
                    var column = await _context.GetGradebookColumnWithToolAsync(id);
                    tool = column?.ResourceLink?.Tool;
                    targetLinkUri = column?.SubmissionReviewUrl.IfMissingThen(tool?.LaunchUrl);
                    break;
                }
                case Constants.Lti.LtiStartProctoringRequestMessageType:
                {
                    // Proctoring 1.0 §4.2: every start is a new attempt, numbered from 1.
                    var resourceLink = await _context.GetResourceLinkAsync(id);
                    if (resourceLink == null || !resourceLink.IsProctored || !int.TryParse(personId, out var candidateId))
                    {
                        _logger.LogError("Resource link {Id} is not a proctored assessment.", id);
                        return BadRequest();
                    }

                    var previousAttempts = await _context.ProctoringAttempts
                        .CountAsync(a => a.ResourceLinkId == id && a.PersonId == candidateId);
                    var attempt = new ProctoringAttempt
                    {
                        ResourceLinkId = id,
                        PersonId = candidateId,
                        AttemptNumber = previousAttempts + 1,
                        SessionData = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(16)),
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ProctoringAttempts.Add(attempt);
                    await _context.SaveChangesAsync();

                    attemptId = attempt.Id;
                    tool = resourceLink.Tool;
                    targetLinkUri = tool.LaunchUrl;
                    break;
                }
                case Constants.Lti.LtiEndAssessmentRequestMessageType:
                {
                    // Sent when the candidate finished, if the proctoring tool asked for it.
                    var attempt = await _context.GetProctoringAttemptAsync(id);
                    tool = attempt?.ResourceLink.Tool;
                    targetLinkUri = tool?.LaunchUrl;
                    attemptId = attempt?.Id;
                    break;
                }
                default:
                    _logger.LogError("Message type {MessageType} not supported.", messageType);
                    return BadRequest();
            }

            if (tool == null)
            {
                _logger.LogError("Tool not found.");
                return BadRequest();
            }

            var values = new Dictionary<string, string>
            {
                // Must equal the "iss" claim of the id_token, or the tool can't match them up.
                ["iss"] = Request.GetIssuer(),
                ["login_hint"] = personId,
                // Must equal the target_link_uri claim of the id_token the tool will receive.
                ["target_link_uri"] = targetLinkUri,
                // Lets the tool pick the right registration when it has several with this platform.
                ["client_id"] = tool.ClientId,
                ["lti_deployment_id"] = tool.DeploymentId,
                ["lti_message_hint"] = JsonSerializer.Serialize(new { id, messageType, courseId, forUserId, attemptId })
            };

            var url = QueryHelpers.AddQueryString(tool.LoginUrl, values);
            _logger.LogInformation("Launching {Tool} using GET {Url}", tool.Name, url);
            return Redirect(url);
        }

        /// <summary>
        /// Returns a <see cref="ContentResult"/> that auto-POSTs the supplied values.
        /// Kept here as a reference implementation for tools that prefer POST-initiated launches.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ContentResult Post(string url, IDictionary<string, string> values)
        {
            var s = new StringBuilder();
            s.Append("<html><head><title></title></head>");
            s.Append("<body onload='document.forms[\"form\"].submit()'>");
            s.Append($"<form name='form' action='{url}' method='post'>");
            foreach (var pair in values)
            {
                s.Append($"<input type='hidden' name='{pair.Key}' value='{pair.Value}' />");
            }

            s.Append("</form></body></html>");
            return new ContentResult
            {
                Content = s.ToString(),
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
