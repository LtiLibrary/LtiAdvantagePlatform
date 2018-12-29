using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Extensions;
using LtiAdvantage.IdentityServer4;
using LtiAdvantage.IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace AdvantagePlatform.Pages.Models
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class ToolModel
    {
        /// <summary>
        /// Parameterless constructor for model binding.
        /// </summary>
        public ToolModel()
        {
        }

        /// <summary>
        /// Create an instance of <see cref="ToolModel"/>.
        /// </summary>
        public ToolModel(HttpContext httpContext)
        {
            // These are the platform's Identity Server properties
            Issuer = httpContext.GetIdentityServerIssuerUri();
            AuthorizeUrl = Issuer.EnsureTrailingSlash() + "connect/authorize";
            JwkSetUrl = Issuer.EnsureTrailingSlash() + ".well-known/openid-configuration/jwks";
            TokenUrl = Issuer.EnsureTrailingSlash() + "connect/token";
        }

        /// <inheritdoc />
        /// <summary>
        /// Create an instance of <see cref="T:AdvantagePlatform.Pages.Models.ToolModel" /> using tool and client entities.
        /// </summary>
        /// <param name="httpContext">The HttpContext.</param>
        /// <param name="tool">The tool entity.</param>
        /// <param name="client">The client entity.</param>
        public ToolModel(HttpContext httpContext, Tool tool, Client client) : this(httpContext)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (client == null) throw new ArgumentNullException(nameof(client));

            // These are the tool's LTI properties
            Id = tool.Id;
            CustomProperties = tool.CustomProperties;
            DeepLinkingLaunchUrl = tool.DeepLinkingLaunchUrl;
            DeploymentId = tool.DeploymentId;
            LaunchUrl = tool.LaunchUrl;
            LoginUrl = tool.LoginUrl;
            Name = tool.Name;

            // These are the tool's Identity Server properties (client)
            IdentityServerClientId = client.Id;
            ClientId = client.ClientId;
            PublicKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PublicPemKey)
                ?.Value;
        }

        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        #region Identity Server Client properties

        /// <summary>
        /// The primary key of the IdentityServer Client associated with the tool.
        /// </summary>
        public int IdentityServerClientId { get; set; }

        /// <summary>
        /// The required Client ID. Must be unique in IdentityServer.
        /// </summary>
        [Required]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        /// <summary>
        /// The client public signing key.
        /// </summary>
        [Required]
        [Display(Name = "Public Key", Description = "Public key to validate messages signed by the tool.")]
        public string PublicKey { get; set; }

        #endregion

        #region Tool properties

        /// <summary>
        /// Custom properties to include with all tool launches.
        /// </summary>
        [Display(Name = "Custom Properties", Description = "Custom properties to include in all launches of this tool deployment.")]
        public string CustomProperties { get; set; }
        
        /// <summary>
        /// Deep linking launch url.
        /// </summary>
        [LocalhostUrl]
        [Display(Name = "Deep Linking Launch URL", Description = "The URL to launch the tool's deep linking experience.")]
        public string DeepLinkingLaunchUrl { get; set; }

        /// <summary>
        /// Generated and immutable deployment id.
        /// </summary>
        [Display(Name = "Deployment ID", Description = "Unique id assigned to this tool deployment.")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// Tool launch url.
        /// </summary>
        [Required]
        [LocalhostUrl]
        [Display(Name = "Launch URL", Description = "The URL to launch the tool.")]
        public string LaunchUrl { get; set; }

        /// <summary>
        /// OIDC login initiation url.
        /// </summary>
        [Required]
        [LocalhostUrl]
        [Display(Name = "Login URL", Description = "The URL to initiate OpenID Connect authorization.")]
        public string LoginUrl { get; set; }

        /// <summary>
        /// Tool display name.
        /// </summary>
        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        #endregion

        #region Identity Server properties

        /// <summary>
        /// Identity Server issuer uri
        /// </summary>
        [Display(Name = "Issuer")]
        public string Issuer { get; set; }

        /// <summary>
        /// Identity Server authorize endpoint url
        /// </summary>
        [Display(Name = "Authorize URL")]
        public string AuthorizeUrl { get; set; }

        /// <summary>
        /// Identity Server JWK Set endpoint url
        /// </summary>
        [Display(Name = "JWK Set URL")]
        public string JwkSetUrl { get; set; }

        /// <summary>
        /// Identity Server access token endpoint uri
        /// </summary>
        [Display(Name = "Access Token URL")]
        public string TokenUrl { get; set; }

        #endregion
    }
}
