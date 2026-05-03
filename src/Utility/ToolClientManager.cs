using System.Collections.Generic;
using System.Threading.Tasks;
using LtiAdvantage;
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

        public async Task CreateAsync(string clientId, string displayName, string launchUrl)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                DisplayName = displayName,
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.Implicit,
                    OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                }
            };

            if (!string.IsNullOrWhiteSpace(launchUrl))
            {
                descriptor.RedirectUris.Add(new System.Uri(launchUrl));
            }

            foreach (var scope in Config.LtiScopes)
            {
                descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            await _applicationManager.CreateAsync(descriptor);
        }

        public async Task UpdateRedirectUriAsync(string clientId, string launchUrl)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null) return;

            var descriptor = new OpenIddictApplicationDescriptor();
            await _applicationManager.PopulateAsync(descriptor, application);

            descriptor.RedirectUris.Clear();
            if (!string.IsNullOrWhiteSpace(launchUrl))
            {
                descriptor.RedirectUris.Add(new System.Uri(launchUrl));
            }

            await _applicationManager.UpdateAsync(application, descriptor);
        }

        public async Task DeleteAsync(string clientId)
        {
            var application = await _applicationManager.FindByClientIdAsync(clientId);
            if (application == null) return;

            await _applicationManager.DeleteAsync(application);
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
