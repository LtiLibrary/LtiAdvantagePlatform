using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;

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
            Issuer = httpContext.Request.GetIssuer();
            AuthorizeUrl = Issuer + "connect/authorize";
            JwkSetUrl = Issuer + ".well-known/jwks";
            TokenUrl = Issuer + "connect/token";
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
            JwksUrl = tool.JwksUrl;
            PublicKey = tool.PublicKey;
            RedirectUris = tool.RedirectUris;
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
        /// The URL of the tool's JSON Web Key Set.
        /// </summary>
        [LocalhostUrl]
        [Display(Name = "Tool JWKS URL", Description = "Where the tool publishes its public keys. The platform fetches them from here, so the tool can rotate its keys.")]
        public string JwksUrl { get; set; }

        /// <summary>
        /// The client public signing key (PEM). Fallback when there is no JWKS URL.
        /// </summary>
        [Display(Name = "Public Key", Description = "Public key to validate messages signed by the tool. Only needed if the tool has no JWKS URL.")]
        public string PublicKey { get; set; }

        #endregion

        #region Tool properties

        [Display(Name = "Custom Properties", Description = "<p>Custom properties to include in all launches of this tool deployment.</p>" + Config.CustomPropertiesHelp)]
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

        [Display(Name = "Redirect URIs", Description = "Other URLs the tool may receive launches at, one per line. The launch URL and deep linking URL are always allowed.")]
        public string RedirectUris { get; set; }

        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; }

        #endregion

        /// <summary>
        /// Checks that the tool has usable keys: a JWKS URL the platform can fetch, or a PEM
        /// public key. Adds model errors and returns null if not; otherwise returns the key
        /// set to store on the tool's OpenIddict application.
        /// </summary>
        public async Task<JsonWebKeySet> ValidateKeysAsync(ModelStateDictionary modelState, ToolSigningKeys toolSigningKeys)
        {
            var jwksUrlField = $"Tool.{nameof(JwksUrl)}";
            var publicKeyField = $"Tool.{nameof(PublicKey)}";

            if (JwksUrl.IsMissing() && PublicKey.IsMissing())
            {
                modelState.AddModelError(jwksUrlField, "Enter the tool's JWKS URL, or its public key.");
                return null;
            }

            if (PublicKey.IsPresent() && PemKeys.ParseRsaPublicKey(PublicKey) == null)
            {
                modelState.AddModelError(publicKeyField, "Enter the tool's PEM-encoded RSA public key.");
                return null;
            }

            var keySet = await toolSigningKeys.GetKeySetAsync(JwksUrl, PublicKey);
            if (keySet == null)
            {
                modelState.AddModelError(jwksUrlField,
                    "Cannot fetch any keys from the JWKS URL. Is the tool running?");
            }

            return keySet;
        }

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
