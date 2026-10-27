using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// LTI 1.3 tools authenticate to the token endpoint with a JWT client assertion as
    /// described in the 1EdTech Security Framework: "typ" is "JWT" and "aud" is the token
    /// endpoint URL. OpenIddict 7 follows the newer RFC 7523bis draft instead and only
    /// accepts "typ": "client-authentication+jwt" with the issuer as "aud". These handlers
    /// also accept the LTI form. The assertion is still checked against the tool's own
    /// public key.
    /// </summary>
    public static class LtiClientAssertionHandlers
    {
        public static OpenIddictServerBuilder AcceptLtiClientAssertions(this OpenIddictServerBuilder builder)
        {
            builder.AddEventHandler<ValidateTokenContext>(descriptor => descriptor
                .UseInlineHandler(context =>
                {
                    if (!IsClientAssertionValidation(context))
                    {
                        return ValueTask.CompletedTask;
                    }

                    if (context.TokenValidationParameters.ValidTypes is { } types)
                    {
                        context.TokenValidationParameters.ValidTypes = types
                            .Append(OpenIddictConstants.JsonWebTokenTypes.GenericJsonWebToken)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();
                    }

                    // Client assertions are only sent to the token endpoint, so the URL of
                    // the current request is the token endpoint URL.
                    if (context.ValidAudiences.Count > 0 && context.RequestUri is { } requestUri)
                    {
                        context.ValidAudiences.Add(requestUri.GetLeftPart(UriPartial.Path));
                    }

                    return ValueTask.CompletedTask;
                })
                .SetOrder(OpenIddictServerHandlers.Protection.ResolveTokenValidationParameters.Descriptor.Order + 1));

            // A "JWT" token is labelled an identity token by default; relabel it.
            builder.AddEventHandler<ValidateTokenContext>(descriptor => descriptor
                .UseInlineHandler(context =>
                {
                    if (IsClientAssertionValidation(context) &&
                        context.Principal?.GetTokenType() == OpenIddictConstants.TokenTypeIdentifiers.IdentityToken)
                    {
                        context.Principal.SetTokenType(OpenIddictConstants.TokenTypeIdentifiers.Private.ClientAssertion);
                    }

                    return ValueTask.CompletedTask;
                })
                .SetOrder(OpenIddictServerHandlers.Protection.ValidateIdentityModelToken.Descriptor.Order + 1));

            return builder;
        }

        private static bool IsClientAssertionValidation(ValidateTokenContext context)
            => context.ValidTokenTypes.Count == 1 &&
               context.ValidTokenTypes.Contains(OpenIddictConstants.TokenTypeIdentifiers.Private.ClientAssertion);
    }
}
