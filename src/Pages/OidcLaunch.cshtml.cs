using System.Collections.Generic;
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
        /// <param name="id">The resource link or tool id.</param>
        /// <param name="messageType">The lti message type.</param>
        /// <param name="courseId">The course id (or null if not launched from a course).</param>
        /// <param name="personId">The person id to impersonate.</param>
        public async Task<IActionResult> OnGetAsync(int id, string messageType, string courseId, string personId)
        {
            Tool tool;

            if (messageType == Constants.Lti.LtiResourceLinkRequestMessageType)
            {
                var resourceLink = await _context.GetResourceLinkAsync(id);
                if (resourceLink == null)
                {
                    _logger.LogError("Resource link not found.");
                    return BadRequest();
                }

                tool = resourceLink.Tool;
            }
            else
            {
                tool = await _context.GetToolAsync(id);
            }

            if (tool == null)
            {
                _logger.LogError("Tool not found.");
                return BadRequest();
            }

            var issuer = $"{Request.Scheme}://{Request.Host}";

            var values = new Dictionary<string, string>
            {
                ["iss"] = issuer,
                ["login_hint"] = personId,
                ["target_link_uri"] = tool.LaunchUrl,
                ["lti_message_hint"] = JsonSerializer.Serialize(new { id, messageType, courseId })
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
