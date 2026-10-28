using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.JsonWebTokens;
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

            // Look the tool's keys up at its JWKS URL, so a rotated key is accepted right away.
            builder.Services.TryAddScoped<ToolJwksClientAssertionHandler>();
            builder.AddEventHandler<ValidateTokenContext>(descriptor => descriptor
                .UseScopedHandler<ToolJwksClientAssertionHandler>()
                .SetOrder(OpenIddictServerHandlers.Protection.ResolveTokenValidationParameters.Descriptor.Order + 2));

            return builder;
        }

        internal static bool IsClientAssertionValidation(ValidateTokenContext context)
            => context.ValidTokenTypes.Count == 1 &&
               context.ValidTokenTypes.Contains(OpenIddictConstants.TokenTypeIdentifiers.Private.ClientAssertion);
    }

    /// <summary>
    /// OpenIddict checks a client assertion against the key set stored on the client's
    /// application. For tools with a JWKS URL this handler uses the keys published there
    /// instead, picked by the assertion's "kid". The stored key set (a copy taken when the
    /// tool was registered) is only used when the JWKS URL cannot be reached.
    /// </summary>
    public class ToolJwksClientAssertionHandler : IOpenIddictServerHandler<ValidateTokenContext>
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolSigningKeys _toolSigningKeys;

        public ToolJwksClientAssertionHandler(ApplicationDbContext context, ToolSigningKeys toolSigningKeys)
        {
            _context = context;
            _toolSigningKeys = toolSigningKeys;
        }

        public async ValueTask HandleAsync(ValidateTokenContext context)
        {
            if (!LtiClientAssertionHandlers.IsClientAssertionValidation(context) ||
                context.TokenValidationParameters == null)
            {
                return;
            }

            // The assertion is not validated yet. Its "iss" (the client id, per the 1EdTech
            // Security Framework) only says whose keys to check the signature against.
            var handler = new JsonWebTokenHandler();
            if (string.IsNullOrEmpty(context.Token) || !handler.CanReadToken(context.Token))
            {
                return;
            }

            var assertion = handler.ReadJsonWebToken(context.Token);
            var tool = await _context.Tools.AsNoTracking()
                .SingleOrDefaultAsync(t => t.ClientId == assertion.Issuer);
            if (tool == null || tool.JwksUrl.IsMissing())
            {
                return;
            }

            var keys = await _toolSigningKeys.GetSigningKeysAsync(tool, assertion.Kid);
            if (keys.Count == 0)
            {
                return;
            }

            context.TokenValidationParameters.IssuerSigningKeyResolver = (_, _, _, _) => keys;
        }
    }
}
