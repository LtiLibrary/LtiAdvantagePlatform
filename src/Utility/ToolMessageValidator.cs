using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Validates an LTI message a tool sends back to the platform through the browser: the
    /// deep linking response, and the proctoring tool's LtiStartAssessment. The tool signs it
    /// with its own key, with its client id as "iss" and the platform's issuer as "aud".
    /// </summary>
    public class ToolMessageValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolSigningKeys _toolSigningKeys;
        private readonly ILogger<ToolMessageValidator> _logger;

        public ToolMessageValidator(
            ApplicationDbContext context,
            ToolSigningKeys toolSigningKeys,
            ILogger<ToolMessageValidator> logger)
        {
            _context = context;
            _toolSigningKeys = toolSigningKeys;
            _logger = logger;
        }

        /// <summary>
        /// Returns the validated token and the tool that sent it, or an error.
        /// </summary>
        public async Task<(JwtSecurityToken Token, Tool Tool, string Error)> ValidateAsync(
            string jwt, string expectedMessageType, HttpRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            if (string.IsNullOrWhiteSpace(jwt) || !handler.CanReadToken(jwt))
            {
                return (null, null, "The message is missing or is not a JWT.");
            }

            var unvalidated = handler.ReadJwtToken(jwt);
            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.ClientId == unvalidated.Issuer);
            if (tool == null)
            {
                return (null, null, $"No registered tool with client id {unvalidated.Issuer}.");
            }

            // The "kid" header names the key the tool signed with. If the tool rotated its keys,
            // an unknown kid makes the platform fetch the tool's JWKS again.
            var keys = await _toolSigningKeys.GetSigningKeysAsync(tool, unvalidated.Header.Kid);
            if (keys.Count == 0)
            {
                return (null, tool, "The tool has no usable public key.");
            }

            var parameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                IssuerSigningKeys = keys,
                ValidateIssuerSigningKey = true,
                ValidIssuer = tool.ClientId,

                // Tools address the message to the "iss" they received in the id_token.
                // Also accept it without the trailing slash, as older tools may send that.
                ValidAudiences = new[] { request.GetIssuer(), request.GetIssuer().TrimEnd('/') }
            };

            try
            {
                handler.ValidateToken(jwt, parameters, out var validated);
                var token = (JwtSecurityToken)validated;

                var messageType = token.Claims.SingleOrDefault(c => c.Type == Constants.LtiClaims.MessageType)?.Value;
                if (messageType != expectedMessageType)
                {
                    return (null, tool, $"Expecting {expectedMessageType}, but found {messageType}.");
                }

                // The deployment must be the one the platform launched the tool with.
                var deploymentId = token.Claims.SingleOrDefault(c => c.Type == Constants.LtiClaims.DeploymentId)?.Value;
                if (deploymentId != null && deploymentId != tool.DeploymentId)
                {
                    return (null, tool, $"Unknown deployment id {deploymentId}.");
                }

                return (token, tool, null);
            }
            catch (Exception e) when (e is SecurityTokenException or ArgumentException)
            {
                _logger.LogError(e, "JWT token validation error.");
                return (null, tool, "The message signature or claims are not valid.");
            }
        }
    }
}
