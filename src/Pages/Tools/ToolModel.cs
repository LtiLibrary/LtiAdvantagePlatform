using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

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

        [Display(Name = "Client ID")]
        public string ClientId { get; set; }

        /// <summary>
        /// The Deployment ID for this Tool/Client combination
        /// </summary>
        [Display(Name = "Deployment ID")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// The Tool name.
        /// </summary>
        [Required]
        [Display(Name = "Tool Name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The URL used to launch the Tool.
        /// </summary>
        [Required]
        [NullableUrl]
        [Display(Name = "Tool URL")]
        public string Url { get; set; }
    }
}
