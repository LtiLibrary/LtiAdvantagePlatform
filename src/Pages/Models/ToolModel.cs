using System;
using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Http;

namespace AdvantagePlatform.Pages.Models
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class ToolModel
    {
        public ToolModel()
        {
        }

        public ToolModel(HttpContext httpContext)
        {
            Issuer = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            AuthorizeUrl = Issuer.EnsureTrailingSlash() + "connect/authorize";
            JwkSetUrl = Issuer.EnsureTrailingSlash() + ".well-known/jwks";
            TokenUrl = Issuer.EnsureTrailingSlash() + "connect/token";
        }

        public ToolModel(HttpContext httpContext, Tool tool) : this(httpContext)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));

            Id = tool.Id;
            ClientId = tool.ClientId;
            CustomProperties = tool.CustomProperties;
            DeepLinkingLaunchUrl = tool.DeepLinkingLaunchUrl;
            DeploymentId = tool.DeploymentId;
            LaunchUrl = tool.LaunchUrl;
            LoginUrl = tool.LoginUrl;
            Name = tool.Name;
            PublicKey = tool.PublicKey;
        }

        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        #region Tool client properties

        /// <summary>
        /// The OAuth/OIDC client id assigned to the tool.
        /// </summary>
        [Required]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        /// <summary>
        /// The client public signing key (PEM).
        /// </summary>
        [Required]
        [Display(Name = "Public Key", Description = "Public key to validate messages signed by the tool.")]
        public string PublicKey { get; set; }

        #endregion

        #region Tool properties

        [Display(Name = "Custom Properties", Description = "Custom properties to include in all launches of this tool deployment.")]
        public string CustomProperties { get; set; }

        [LocalhostUrl]
        [Display(Name = "Deep Linking Launch URL", Description = "The URL to launch the tool's deep linking experience.")]
        public string DeepLinkingLaunchUrl { get; set; }

        [Display(Name = "Deployment ID", Description = "Unique id assigned to this tool deployment.")]
        public string DeploymentId { get; set; }

        [Required]
        [LocalhostUrl]
        [Display(Name = "Launch URL", Description = "The URL to launch the tool.")]
        public string LaunchUrl { get; set; }

        [Required]
        [LocalhostUrl]
        [Display(Name = "Login URL", Description = "The URL to initiate OpenID Connect authorization.")]
        public string LoginUrl { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        #endregion

        #region Platform OIDC metadata

        [Display(Name = "Issuer")]
        public string Issuer { get; set; }

        [Display(Name = "Authorize URL")]
        public string AuthorizeUrl { get; set; }

        [Display(Name = "JWK Set URL")]
        public string JwkSetUrl { get; set; }

        [Display(Name = "Access Token URL")]
        public string TokenUrl { get; set; }

        #endregion
    }
}
