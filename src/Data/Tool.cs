using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvantagePlatform.Data
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class Tool
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The OAuth/OIDC client id of the registered tool. Matches the OpenIddict
        /// application client_id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The URL where the tool publishes its public signing keys (a JSON Web Key Set).
        /// Used to verify deep-linking responses and the JWT client assertions sent to the
        /// token endpoint. Preferred over <see cref="PublicKey"/>, because the tool can
        /// rotate its keys without the platform being updated.
        /// </summary>
        public string JwksUrl { get; set; }

        /// <summary>
        /// The tool's public signing key, in PEM format. Fallback for tools without a
        /// <see cref="JwksUrl"/>.
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Custom properties included with every resource link that uses this tool.
        /// </summary>
        public string CustomProperties { get; set; }

        /// <summary>
        /// The URL used to launch the deep linking flow.
        /// </summary>
        public string DeepLinkingLaunchUrl { get; set; }

        /// <summary>
        /// The Deployment ID for this Tool/Client combination.
        /// </summary>
        public string DeploymentId { get; set; }

        /// <summary>
        /// The URL used to launch the tool.
        /// </summary>
        public string LaunchUrl { get; set; }

        /// <summary>
        /// The URL used to initiate OIDC authorization.
        /// </summary>
        public string LoginUrl { get; set; }

        /// <summary>
        /// Extra URLs the tool may ask the platform to send launches to, one per line.
        /// The launch URL and deep linking URL are always allowed. Filled in by Dynamic
        /// Registration from the tool's redirect_uris.
        /// </summary>
        public string RedirectUris { get; set; }

        /// <summary>
        /// The tool name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Every URL the tool may receive an id_token at: the OpenIddict client's redirect URIs.
        /// </summary>
        public IEnumerable<string> GetAllRedirectUris()
        {
            var extra = (RedirectUris ?? string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return new[] { LaunchUrl, DeepLinkingLaunchUrl }.Concat(extra)
                .Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct();
        }
    }
}
