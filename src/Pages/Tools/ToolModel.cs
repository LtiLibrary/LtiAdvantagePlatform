using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Utility;

namespace AdvantagePlatform.Pages.Tools
{
    /// <summary>
    /// This Tool implements the "2.1.2.2 Tool registered and deployed" model shown in
    /// Figure 2 within https://www.imsglobal.org/spec/lti/v1p3#tool-registered-and-deployed
    /// </summary>
    public class ToolModel
    {
        public int Id { get; set; }

        /// <summary>
        /// The ID of the IdentityServer Client associated with this Tool
        /// </summary>
        public int IdentityServerClientId { get; set; }

        [Display(Name = "Deployment ID", Description = "This deployment ID will be sent with all launch messages from the platform.")]
        public string DeploymentId { get; set; }

        [Required]
        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        [Display(Name = "Client Secret", Description = "This is the shared secret to use if your tool sends a shared secret for client credentials.")]
        public string ClientSecret { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Private Key", Description = "This is the private key to use if your tool sends a signed JWT as client credentials.")]
        public string PrivateKey { get; set; }

        [Display(Name = "Public Key")]
        public string PublicKey { get; set; }
        
        [Required]
        [NullableUrl]
        [Display(Name = "Launch URL")]
        public string Url { get; set; }
    }
}
