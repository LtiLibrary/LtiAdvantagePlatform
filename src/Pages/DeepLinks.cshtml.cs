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
        private readonly ToolSigningKeys _toolSigningKeys;
        private readonly ILogger<DeepLinksModel> _logger;

        public DeepLinksModel(
            ApplicationDbContext context,
            ToolSigningKeys toolSigningKeys,
            IConfiguration configuration,
            ILogger<DeepLinksModel> logger)
        {
            _context = context;
            _toolSigningKeys = toolSigningKeys;
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

            if (!await ValidateToken(IdToken))
            {
                return Unauthorized();
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(IdToken);

            var messageType = token.Claims.SingleOrDefault(c => c.Type == Constants.LtiClaims.MessageType)?.Value;
            if (messageType != Constants.Lti.LtiDeepLinkingResponseMessageType)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Incorrect message format",
                    Detail = $"Expecting {Constants.Lti.LtiDeepLinkingResponseMessageType}, but found {messageType}."
                });
            }

            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.ClientId == token.Issuer);
            if (tool == null)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Unknown tool",
                    Detail = $"No registered tool with client id {token.Issuer}."
                });
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
            course.GradebookColumns.Add(new GradebookColumn
            {
                Label = item.LineItem?.Label ?? resourceLink.Title,
                ResourceId = item.LineItem?.ResourceId,
                ResourceLink = resourceLink,
                ScoreMaximum = item.LineItem?.ScoreMaximum ?? 100,
                Tag = item.LineItem?.Tag.IfMissingThen("Deep Link"),
                StartDateTime = item.Submission?.StartDateTime,
                EndDateTime = item.Submission?.EndDateTime
            });
        }

        private static void AddLinkedContent(ContentItem item, Platform platform, Course course)
        {
            var content = new LinkedContent
            {
                Type = item.Type,
                Title = item.Title,
                Text = item.Text,
                Url = item.Url,
                Html = item.Html,
                ThumbnailUrl = item.Thumbnail?.Url,
                DocumentTarget = item.Iframe != null ? "iframe" : "window",
                Width = item.Iframe?.Width,
                Height = item.Iframe?.Height
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

        private async Task<bool> ValidateToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(idToken))
            {
                _logger.LogError("Cannot read {IdToken}.", nameof(idToken));
                return false;
            }

            var token = handler.ReadJwtToken(idToken);
            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.ClientId == token.Issuer);
            if (tool == null)
            {
                _logger.LogError("Cannot find tool for issuer {Issuer}.", token.Issuer);
                return false;
            }

            // The "kid" header names the key the tool signed with. If the tool rotated its keys,
            // an unknown kid makes the platform fetch the tool's JWKS again.
            var keys = await _toolSigningKeys.GetSigningKeysAsync(tool, token.Header.Kid);
            if (keys.Count == 0)
            {
                _logger.LogError("Tool {ToolId} has no usable public key.", tool.Id);
                return false;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                RequireExpirationTime = true,

                IssuerSigningKeys = keys,
                ValidateIssuerSigningKey = true,

                ValidateIssuer = false,

                // Tools address the response to the "iss" they received in the id_token.
                // Also accept it without the trailing slash, as older tools may send that.
                ValidAudiences = new[] { Request.GetIssuer(), Request.GetIssuer().TrimEnd('/') },
                ValidateAudience = true
            };

            try
            {
                handler.ValidateToken(idToken, tokenValidationParameters, out _);
                return true;
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "JWT token validation error.");
                return false;
            }
        }
    }
}
