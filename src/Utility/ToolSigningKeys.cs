using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantage.IdentityModel.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Finds the public keys a tool signs with. The platform needs them to verify deep linking
    /// responses and the client assertions tools send to /connect/token.
    ///
    /// Tools publish their keys at a JWKS URL. That is the preferred source: the keys are cached,
    /// and a token signed with a key id ("kid") the cache does not know yet makes it fetch the
    /// set again, so a tool can rotate its key without anyone touching the platform. A pasted
    /// PEM public key is still supported as a fallback.
    /// </summary>
    public class ToolSigningKeys
    {
        private readonly JwksCache _jwksCache;
        private readonly ILogger<ToolSigningKeys> _logger;

        public ToolSigningKeys(JwksCache jwksCache, ILogger<ToolSigningKeys> logger)
        {
            _jwksCache = jwksCache;
            _logger = logger;
        }

        /// <summary>
        /// Returns the keys that may have signed a token from <paramref name="tool"/>.
        /// </summary>
        /// <param name="tool">The tool that sent the token.</param>
        /// <param name="keyId">The token's "kid" header, used to pick up a rotated key.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<IReadOnlyCollection<SecurityKey>> GetSigningKeysAsync(
            Tool tool, string keyId, CancellationToken cancellationToken = default)
            => GetSigningKeysAsync(tool.JwksUrl, tool.PublicKey, keyId, cancellationToken);

        /// <summary>
        /// Returns the keys published at <paramref name="jwksUrl"/>, or the PEM key if there is no
        /// JWKS URL or its keys cannot be fetched. Returns an empty list if neither works.
        /// </summary>
        public async Task<IReadOnlyCollection<SecurityKey>> GetSigningKeysAsync(
            string jwksUrl, string publicKeyPem, string keyId, CancellationToken cancellationToken = default)
        {
            if (jwksUrl.IsPresent())
            {
                try
                {
                    var keys = await _jwksCache.GetSigningKeysAsync(jwksUrl, keyId, cancellationToken);
                    if (keys.Count > 0)
                    {
                        return keys;
                    }

                    _logger.LogWarning("The JWKS at {JwksUrl} has no signing keys.", jwksUrl);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _logger.LogWarning(e, "Cannot fetch the tool's keys from {JwksUrl}.", jwksUrl);
                }
            }

            var pemKey = PemKeys.ParseRsaPublicKey(publicKeyPem);
            return pemKey == null ? Array.Empty<SecurityKey>() : new SecurityKey[] { pemKey };
        }

        /// <summary>
        /// Returns the tool's current keys as a <see cref="JsonWebKeySet"/> to store on its
        /// OpenIddict application, or null if there are none.
        /// </summary>
        /// <remarks>
        /// OpenIddict only accepts a confidential client that has a client secret or a key set,
        /// so the application keeps a copy of the keys. At the token endpoint the keys are looked
        /// up again (see <see cref="ToolJwksClientAssertionHandler"/>), so this copy only matters
        /// if the JWKS URL cannot be reached.
        /// </remarks>
        public async Task<JsonWebKeySet> GetKeySetAsync(
            string jwksUrl, string publicKeyPem, CancellationToken cancellationToken = default)
        {
            var keys = await GetSigningKeysAsync(jwksUrl, publicKeyPem, keyId: null, cancellationToken);

            var keySet = new JsonWebKeySet();
            foreach (var key in keys)
            {
                try
                {
                    keySet.Keys.Add(JsonWebKeyConverter.ConvertFromSecurityKey(key));
                }
                catch (NotSupportedException e)
                {
                    _logger.LogWarning(e, "Skipping key {KeyId}.", key.KeyId);
                }
            }

            return keySet.Keys.Any() ? keySet : null;
        }
    }
}
