using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using LtiAdvantageLibrary.NetCore;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform
{
    /// <inheritdoc />
    /// <summary>
    /// Validates a secret based on RS256 signed JWT token
    /// </summary>
    public class SignedJwtSecretValidator : ISecretValidator
    {
        private readonly ILogger<SignedJwtSecretValidator> _logger;
        private readonly string _audienceUri;

        /// <summary>
        /// Instantiates an instance of Signed JWT secret validator
        /// </summary>
        public SignedJwtSecretValidator(IHttpContextAccessor contextAccessor, ILogger<SignedJwtSecretValidator> logger)
        {
            _audienceUri = contextAccessor.HttpContext.GetIdentityServerIssuerUri();
            _logger = logger;
        }

        /// <inheritdoc />
        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="T:System.ArgumentException">ParsedSecret.Credential is not a JWT token</exception>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });
            var success = Task.FromResult(new SecretValidationResult { Success = true });

            if (parsedSecret.Type != IdentityServerConstants.ParsedSecretTypes.JwtBearer)
            {
                return fail;
            }

            if (!(parsedSecret.Credential is string jwt))
            {
                _logger.LogError("ParsedSecret.Credential is not a string.");
                return fail;
            }

            var publicKeys = secrets
                .Where(s => s.Type == Constants.SecretTypes.PublicKey)
                .Select(s => new RsaSecurityKey(RsaHelper.PublicKeyFromPemString(s.Value)))
                .ToList();

            if (!publicKeys.Any())
            {
                _logger.LogError("There are no public keys available to validate client assertion.");
                return fail;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = publicKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = parsedSecret.Id,
                ValidateIssuer = true,

                // Accept either the base uri or the token endpoint url
                ValidAudiences = new []
                {
                    _audienceUri, 
                    string.Concat(_audienceUri.EnsureTrailingSlash(), Constants.ProtocolRoutePaths.Token)
                },
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(jwt, tokenValidationParameters, out var token);

                var jwtToken = (JwtSecurityToken)token;
                if (jwtToken.Subject != jwtToken.Issuer)
                {
                    _logger.LogError("Both 'sub' and 'iss' in the client assertion token must have a value of client_id.");
                    return fail;
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "JWT token validation error");
                return fail;
            }
        }
    }
}
