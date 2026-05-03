using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.DeepLinking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace AdvantagePlatform.Pages
{
    [IgnoreAntiforgeryToken]
    public class DeepLinksModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeepLinksModel> _logger;

        public DeepLinksModel(
            ApplicationDbContext context,
            ILogger<DeepLinksModel> logger)
        {
            _context = context;
            _logger = logger;
        }

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

            var platform = await _context.GetPlatformAsync(platformId);
            var course = courseId.HasValue ? await _context.GetCourseAsync(courseId.Value) : null;

            var ltiRequest = new LtiDeepLinkingResponse(token.Payload);
            var contentItems = ltiRequest.ContentItems;
            if (contentItems != null)
            {
                foreach (var contentItem in contentItems)
                {
                    if (contentItem.Type != Constants.ContentItemTypes.LtiLink)
                    {
                        continue;
                    }

                    var ltiLink = (ILtiLinkItem) contentItem;

                    var resourceLink = new ResourceLink
                    {
                        CustomProperties = ltiLink.Custom.ToDatabaseString(),
                        Description = ltiLink.Text,
                        Title = ltiLink.Title,
                        Tool = tool
                    };

                    if (course == null)
                    {
                        platform.ResourceLinks.Add(resourceLink);
                    }
                    else
                    {
                        course.ResourceLinks.Add(resourceLink);
                        course.GradebookColumns.Add(new GradebookColumn
                        {
                            Label = ltiLink.LineItem?.Label ?? resourceLink.Title,
                            ResourceId = ltiLink.LineItem?.ResourceId,
                            ResourceLink = resourceLink,
                            ScoreMaximum = ltiLink.LineItem?.ScoreMaximum ?? 100,
                            Tag = ltiLink.LineItem?.Tag.IfMissingThen("Deep Link")
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Page();
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

            var keys = GetPemKeys(tool.PublicKey).ToList();
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

                ValidAudience = $"{Request.Scheme}://{Request.Host}",
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

        /// <summary>
        /// Convert a PEM-encoded RSA public key into a <see cref="RsaSecurityKey"/>.
        /// </summary>
        private static IEnumerable<RsaSecurityKey> GetPemKeys(string pemKey)
        {
            if (string.IsNullOrWhiteSpace(pemKey))
            {
                yield break;
            }

            using var keyTextReader = new StringReader(pemKey);
            if (new PemReader(keyTextReader).ReadObject() is RsaKeyParameters bouncyKeyParameters)
            {
                var rsaParameters = new RSAParameters
                {
                    Modulus = bouncyKeyParameters.Modulus.ToByteArrayUnsigned(),
                    Exponent = bouncyKeyParameters.Exponent.ToByteArrayUnsigned()
                };

                yield return new RsaSecurityKey(rsaParameters);
            }
        }
    }
}
