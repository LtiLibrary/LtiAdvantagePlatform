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

        [Display(Name = "Deployment ID")]
        public string DeploymentId { get; set; }

        [Required]
        [Display(Name = "Tool Client ID")]
        public string ToolClientId { get; set; }

        [Required]
        [Display(Name = "Tool Issuer")]
        public string ToolIssuer { get; set; }

        [NullableUrl]
        [Display(Name = "Tool JSON Web Keys URL")]
        public string ToolJsonWebKeysUrl { get; set; }

        [Required]
        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }
        
        [Required]
        [NullableUrl]
        [Display(Name = "Tool URL")]
        public string ToolUrl { get; set; }
    }
}
