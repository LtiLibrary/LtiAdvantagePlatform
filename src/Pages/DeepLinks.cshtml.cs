using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Extensions;
using LtiAdvantage;
using LtiAdvantage.DeepLinking;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;

namespace AdvantagePlatform.Pages
{
    [IgnoreAntiforgeryToken]
    public class DeepLinksModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly ILogger<DeepLinksModel> _logger;

        public DeepLinksModel(
            ApplicationDbContext context, 
            IConfigurationDbContext identityContext,
            ILogger<DeepLinksModel> logger)
        {
            _context = context;
            _identityContext = identityContext;
            _logger = logger;
        }

        [BindProperty(Name = "id_token")]
        public string IdToken { get; set; }

        [BindProperty(Name = "JWT")]
        public string Jwt
        {
            get { return IdToken; }
            set { IdToken = value; }
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

            var client = await _identityContext.Clients.SingleOrDefaultAsync(c => c.ClientId == token.Issuer);
            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.IdentityServerClientId == client.Id);
            var platform = await _context.GetPlatformAsync(platformId);
            var course = courseId.HasValue ? await _context.GetCourseAsync(courseId.Value) : null;

            var ltiRequest = new LtiDeepLinkingResponse(token.Payload);
            var contentItems = ltiRequest.ContentItems;
            if (contentItems != null)
            {
                foreach (var contentItem in contentItems)
                {
                    // Can only handle LTI Links
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
                _logger.LogError($"Cannot read {nameof(idToken)}");
                return false;
            }

            var token = handler.ReadJwtToken(idToken);
            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.ClientId == token.Issuer);
            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.IdentityServerClientId == client.Id);
            if (client == null || tool == null)
            {
                _logger.LogError($"Cannot find {token.Issuer}.");
                return false;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The token must be signed to prove the client credentials.
                RequireSignedTokens = true,
                RequireExpirationTime = true,

                IssuerSigningKeys = GetPemKeys(client.ClientSecrets),
                ValidateIssuerSigningKey = true,

                // Already checked issuer above
                ValidateIssuer = false,

                ValidAudience = HttpContext.GetIdentityServerIssuerUri(),
                ValidateAudience = true
            };

            try
            {
                handler.ValidateToken(idToken, tokenValidationParameters, out _);

                return true;
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "JWT token validation error");
                return false;
            }
        }
        
        /// <summary>
        /// Get the PEM format secrets.
        /// </summary>
        /// <param name="secrets">The secrets to examine.</param>
        /// <returns>The PEM secrets converted into <see cref="RsaSecurityKey"/>'s.</returns>
        private static IEnumerable<RsaSecurityKey> GetPemKeys(IEnumerable<ClientSecret> secrets)
        {
            var pemKeys = secrets
                .Where(s => s.Type == LtiAdvantage.IdentityServer4.Validation.Constants.SecretTypes.PublicPemKey)
                .Select(s => s.Value)
                .ToList();

            var rsaSecurityKeys = new List<RsaSecurityKey>();

            foreach (var pemKey in pemKeys)
            {
                using (var keyTextReader = new StringReader(pemKey))
                {
                    // PemReader can read any PEM file. Only interested in RsaKeyParameters.
                    if (new PemReader(keyTextReader).ReadObject() is RsaKeyParameters bouncyKeyParameters)
                    {
                        var rsaParameters = new RSAParameters
                        {
                            Modulus = bouncyKeyParameters.Modulus.ToByteArrayUnsigned(),
                            Exponent = bouncyKeyParameters.Exponent.ToByteArrayUnsigned()
                        };

                        var rsaSecurityKey = new RsaSecurityKey(rsaParameters);

                        rsaSecurityKeys.Add(rsaSecurityKey);
                    }
                }
            }

            return rsaSecurityKeys;
        }
    }
}