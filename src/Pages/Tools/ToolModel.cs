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

        [Display(Name = "Deployment ID", Description = "This Deployment ID will be sent with all launch messages.")]
        public string DeploymentId { get; set; }

        [Required]
        [Display(Name = "Client ID")]
        public string ToolClientId { get; set; }

        [Display(Name = "Client Secret")]
        public string ToolClientSecret { get; set; }

        [Required]
        [Display(Name = "Issuer", Description = "This is the Issuer for all messages that originate from the Tool.")]
        public string ToolIssuer { get; set; }

        [NullableUrl]
        [Display(Name = "JSON Web Keys URL")]
        public string ToolJsonWebKeysUrl { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string ToolName { get; set; }
        
        [Required]
        [NullableUrl]
        [Display(Name = "Launch URL")]
        public string ToolUrl { get; set; }
    }
}
