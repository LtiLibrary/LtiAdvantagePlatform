using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Spa;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.DeepLinking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Pages
{
    [IgnoreAntiforgeryToken]
    public class DeepLinksModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolMessageValidator _toolMessageValidator;
        private readonly ILogger<DeepLinksModel> _logger;

        public DeepLinksModel(
            ApplicationDbContext context,
            ToolMessageValidator toolMessageValidator,
            IConfiguration configuration,
            ILogger<DeepLinksModel> logger)
        {
            _context = context;
            _toolMessageValidator = toolMessageValidator;
            _logger = logger;

            // The page tells the window that opened the deep linking flow (the Razor home page or
            // the React app) that it is done. Only these origins may receive that message.
            MessageTargetOrigins = configuration.GetSpaOrigins();
        }

        /// <summary>
        /// Number of content items (LTI links and other content) the tool sent back and the platform saved.
        /// </summary>
        public int AddedCount { get; private set; }

        /// <summary>
        /// The response's msg: a message from the tool for the user.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The response's errormsg: an error message from the tool for the user.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Extra origins (besides this platform's own) that may receive the "done" message.
        /// </summary>
        public string[] MessageTargetOrigins { get; }

        [BindProperty(Name = "id_token")]
        public string IdToken { get; set; }

        [BindProperty(Name = "JWT")]
        public string Jwt
        {
            get => IdToken;
            set => IdToken = value;
        }

        public async Task<IActionResult> OnPost(int platformId, int? courseId = null)
        {
            if (IdToken.IsMissing())
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Missing data",
                    Detail = $"{nameof(IdToken)} is missing."
                });
            }

            var (token, tool, error) = await _toolMessageValidator.ValidateAsync(
                IdToken, Constants.Lti.LtiDeepLinkingResponseMessageType, Request);
            if (token == null)
            {
                _logger.LogError("Deep linking response rejected: {Error}", error);
                return BadRequest(new ProblemDetails { Title = "Invalid deep linking response", Detail = error });
            }

            // The return URL names the platform and course. Only save items there when the tool
            // belongs to the same user, so a tool can't add content to someone else's course.
            var owner = await _context.GetUserByPlatformIdAsync(platformId);
            if (owner == null || owner.Tools.All(t => t.Id != tool.Id) ||
                (courseId.HasValue && owner.Course?.Id != courseId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unknown platform or course",
                    Detail = "The deep linking return URL does not match this tool."
                });
            }

            var platform = await _context.GetPlatformAsync(platformId);
            var course = courseId.HasValue ? await _context.GetCourseAsync(courseId.Value) : null;

            var response = new LtiDeepLinkingResponse(token.Payload);

            // msg and errormsg are meant for the user, log and errorlog for the platform's logs
            // (https://www.imsglobal.org/spec/lti-dl/v2p0#deep-linking-response-message).
            Message = response.Message;
            ErrorMessage = response.ErrorMessage;
            if (response.Log.IsPresent())
            {
                _logger.LogInformation("Deep linking log from {Tool}: {Log}", tool.Name, response.Log);
            }
            if (response.ErrorLog.IsPresent())
            {
                _logger.LogWarning("Deep linking error log from {Tool}: {ErrorLog}", tool.Name, response.ErrorLog);
            }

            foreach (var contentItem in response.ContentItems ?? Array.Empty<ContentItem>())
            {
                switch (contentItem.Type)
                {
                    case Constants.ContentItemTypes.LtiLink:
                        AddResourceLink(contentItem, tool, platform, course);
                        break;

                    case Constants.ContentItemTypes.Link:
                    case Constants.ContentItemTypes.Html:
                    case Constants.ContentItemTypes.File:
                    case Constants.ContentItemTypes.Image:
                        AddLinkedContent(contentItem, platform, course);
                        break;

                    default:
                        // Only types listed in accept_types may come back; ignore anything else.
                        _logger.LogWarning("Ignoring content item of type {Type}.", contentItem.Type);
                        continue;
                }

                AddedCount++;
            }

            await _context.SaveChangesAsync();

            return Page();
        }

        private static void AddResourceLink(ContentItem item, Tool tool, Platform platform, Course course)
        {
            var resourceLink = new ResourceLink
            {
                CustomProperties = item.Custom?.ToDatabaseString(),
                Description = item.Text,
                Title = item.Title,
                Tool = tool,
                // An "iframe" property asks the platform to embed the tool; "window" (or nothing)
                // means a new window. Both are in accept_presentation_document_targets.
                DocumentTarget = item.Iframe != null ? "iframe" : "window",
                IframeHeight = item.Iframe?.Height
            };

            if (course == null)
            {
                platform.ResourceLinks.Add(resourceLink);
                return;
            }

            course.ResourceLinks.Add(resourceLink);

            // This sample gives every course link a gradebook column. When the tool sent a
            // lineItem, the column takes its label, scoreMaximum, resourceId and tag, so the
            // tool can find it again with the line items service.
            var column = new GradebookColumn
            {
                Label = item.LineItem?.Label ?? resourceLink.Title,
                ResourceId = item.LineItem?.ResourceId,
                ResourceLink = resourceLink,
                ScoreMaximum = item.LineItem?.ScoreMaximum ?? 100,
                Tag = item.LineItem?.Tag.IfMissingThen("Deep Link"),
                StartDateTime = item.Submission?.StartDateTime,
                EndDateTime = item.Submission?.EndDateTime,
                // AGS 2.0 gradesReleased, sent with the deep-linked lineItem.
                GradesReleased = item.LineItem?.GradesReleased
            };
            // Submission Review 1.0: a deep-linked lineItem can ask for submission review up front,
            // so the gradebook offers "View submission" without a later line items call.
            column.SetSubmissionReview(item.LineItem?.SubmissionReview);
            course.GradebookColumns.Add(column);
        }

        private static void AddLinkedContent(ContentItem item, Platform platform, Course course)
        {
            var content = new LinkedContent
            {
                Type = item.Type,
                Title = item.Title,
                Text = item.Text,
                Url = item.Url,
                // A link item with "embed" carries HTML for the platform to show in its page. It is
                // the most specific request, so it wins over an iframe or window property.
                Html = item.Html ?? (item.Embed.IsPresent() ? item.Embed : null),
                ThumbnailUrl = item.Thumbnail?.Url,
                DocumentTarget = item.Embed.IsPresent() ? "embed" : item.Iframe != null ? "iframe" : "window",
                // The iframe's size, or the item's own (an image's width and height).
                Width = item.Iframe?.Width ?? item.Width,
                Height = item.Iframe?.Height ?? item.Height
            };

            if (course == null)
            {
                platform.LinkedContent.Add(content);
            }
            else
            {
                course.LinkedContent.Add(content);
            }
        }
    }
}
