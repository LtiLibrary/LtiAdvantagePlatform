using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.AspNetCore.OpenIdConnect;
using LtiAdvantage.DynamicRegistration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AdvantagePlatform.Controllers
{
    /// <summary>
    /// Publishes /.well-known/openid-configuration with the LTI platform configuration
    /// claim that Dynamic Registration needs (https://www.imsglobal.org/spec/lti-dr/v1p0#platform-configuration).
    /// OpenIddict's own discovery endpoint is turned off in Program.cs, so this is the only
    /// document at that address.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OpenIdConfigurationController : OpenIdConfigurationControllerBase
    {
        public OpenIdConfigurationController(ILogger<OpenIdConfigurationController> logger) : base(logger)
        {
        }

        protected override Task<PlatformOpenIdConfiguration> OnGetOpenIdConfigurationAsync()
        {
            // The issuer must be exactly the "iss" of the platform's id_tokens, and the tool
            // checks that this document's URL starts with it (DR §3.5.1).
            var issuer = Request.GetIssuer();

            return Task.FromResult(new PlatformOpenIdConfiguration
            {
                Issuer = issuer,
                AuthorizationEndpoint = issuer + "connect/authorize",
                TokenEndpoint = issuer + "connect/token",
                TokenEndpointAuthMethodsSupported = new[] { "private_key_jwt" },
                TokenEndpointAuthSigningAlgValuesSupported = new[] { "RS256" },
                JwksUri = issuer + ".well-known/jwks",
                RegistrationEndpoint = Url.Link(DynamicRegistrationController.RouteName, null),
                ScopesSupported = Config.LtiScopes.ToList(),
                ResponseTypesSupported = new[] { OpenIddictConstants.ResponseTypes.IdToken },
                SubjectTypesSupported = new[] { OpenIddictConstants.SubjectTypes.Public },
                IdTokenSigningAlgValuesSupported = new[] { "RS256" },
                ClaimsSupported = new[]
                {
                    OpenIddictConstants.Claims.Subject,
                    OpenIddictConstants.Claims.Issuer,
                    Constants.OidcClaims.Name,
                    Constants.OidcClaims.GivenName,
                    Constants.OidcClaims.FamilyName,
                    Constants.OidcClaims.Email
                },
                LtiPlatformConfiguration = new LtiPlatformConfiguration
                {
                    ProductFamilyCode = Config.ProductFamilyCode,
                    Version = Config.PlatformVersion,
                    MessagesSupported = Config.MessagesSupported
                        .Select(type => new MessageDescriptor { Type = type })
                        .ToList(),
                    // Custom parameter substitutions LtiAdvantageClaimsHandler fills in.
                    Variables = Config.SupportedVariables.ToList()
                }
            });
        }
    }
}
