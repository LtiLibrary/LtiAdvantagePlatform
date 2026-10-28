using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace AdvantagePlatform.Utility
{
    /// <summary>
    /// Wraps <see cref="IOpenIddictApplicationManager"/> with the LTI Advantage
    /// defaults the sample uses for every registered tool.
    /// </summary>
    public class ToolClientManager
    {
        private readonly IOpenIddictApplicationManager _applicationManager;

        public ToolClientManager(IOpenIddictApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        public async Task<bool> ExistsAsync(string clientId)
        {
            return await _applicationManager.FindByClientIdAsync(clientId) is not null;
        }

        public async Task CreateAsync(string clientId, string displayName, IEnumerable<string> redirectUris, JsonWebKeySet keySet)
        {
            var descriptor = new OpenIddictApplicationDescriptor { ClientId = clientId };
            ApplyToolSettings(descriptor, displayName, redirectUris, keySet);
            await _applicationManager.CreateAsync(descriptor);
        }

        /// <summary>
        /// Brings the tool's OpenIddict application in line with the tool settings,
        /// creating it if it does not exist yet.
        /// </summary>
        public async Task UpdateAsync(string clientId, string displayName, IEnumerable<string> redirectUris, JsonWebKeySet keySet)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null)
            {
                await CreateAsync(clientId, displayName, redirectUris, keySet);
                return;
            }

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, application);
            ApplyToolSettings(descriptor, displayName, redirectUris, keySet);
            await _applicationManager.UpdateAsync(application, descriptor);
        }

        public async Task DeleteAsync(string clientId)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null) return;

            await _applicationManager.DeleteAsync(application);
        }

        private static void ApplyToolSettings(
            OpenIddictApplicationDescriptor descriptor,
            string displayName,
            IEnumerable<string> redirectUris,
            JsonWebKeySet keySet)
        {
            descriptor.DisplayName = displayName;

            // LTI tools authenticate to the token endpoint with a JWT signed by their
            // private key (private_key_jwt), so they are confidential clients whose
            // credential is their public key rather than a client secret.
            // (ToolSigningKeys builds the key set from the tool's JWKS URL or PEM key.)
            descriptor.ClientType = OpenIddictConstants.ClientTypes.Confidential;
            descriptor.ConsentType = OpenIddictConstants.ConsentTypes.Implicit;
            descriptor.JsonWebKeySet = keySet;

            descriptor.RedirectUris.Clear();
            foreach (var redirectUri in redirectUris.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct())
            {
                descriptor.RedirectUris.Add(new Uri(redirectUri));
            }

            descriptor.Permissions.Clear();
            descriptor.Permissions.UnionWith(new[]
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.GrantTypes.Implicit,
                OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
            });
            foreach (var scope in Config.LtiScopes)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }
        }

        public async Task EnsureSwaggerClientAsync()
        {
            const string clientId = "swagger";
            if (await _applicationManager.FindByClientIdAsync(clientId) is not null)
            {
                return;
            }

            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ClientSecret = "secret",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                DisplayName = "Swagger UI",
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                }
            };

            foreach (var scope in Config.LtiScopes)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            await _applicationManager.CreateAsync(descriptor);
        }

        public IReadOnlyCollection<string> Scopes => (IReadOnlyCollection<string>)Config.LtiScopes;
    }
}
