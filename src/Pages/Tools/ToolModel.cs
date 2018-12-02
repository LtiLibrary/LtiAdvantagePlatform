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

namespace AdvantagePlatform.Pages.Tools
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

        /// <summary>
        /// Create an instance of <see cref="ToolModel"/> using tool and client entities.
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
            Name = tool.Name;
            LaunchUrl = tool.LaunchUrl;
            LoginUrl = tool.LoginUrl;
            CustomProperties = tool.CustomProperties;
            DeploymentId = tool.DeploymentId;

            // These are the tool's Identity Server properties (client)
            IdentityServerClientId = client.Id;
            ClientId = client.ClientId;
            PrivateKey = client.ClientSecrets
                ?.FirstOrDefault(s => s.Type == Constants.SecretTypes.PrivatePemKey)
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
        /// The client private signing key.
        /// </summary>
        [Required]
        [Display(Name = "Private Key", Description = "Private key to sign messages sent by the tool.")]
        public string PrivateKey { get; set; }

        #endregion

        #region Tool properties

        /// <summary>
        /// Custom properties to include with all tool launches.
        /// </summary>
        [Display(Name = "Custom Properties", Description = "Custom properties to include in all launches of this tool deployment.")]
        public string CustomProperties { get; set; }
        
        /// <summary>
        /// Generated and immutable deployment id.
        /// </summary>
        [Display(Name = "Deployment ID", Description = "Unique ID assigned to this tool deployment.")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// Tool launch url.
        /// </summary>
        [Required]
        [LocalhostUrl]
        [Display(Name = "Launch URL")]
        public string LaunchUrl { get; set; }

        /// <summary>
        /// OIDC login initiation url.
        /// </summary>
        [Required]
        [LocalhostUrl]
        [Display(Name = "Login URL", Description = "The endpoint URL to initiate OpenID Connect authorization.")]
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
