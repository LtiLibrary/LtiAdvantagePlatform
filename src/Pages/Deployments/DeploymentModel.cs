using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Deployments
{
    public class DeploymentModel
    {
        public int Id { get; set; }

        [Display(Name = "Client Name")]
        public string ClientName { get; set; }

        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }

        [Display(Name = "Tool Placement")]
        public Deployment.ToolPlacements? ToolPlacement { get; set; }

        [Display(Name = "Tool URL")]
        public string ToolUrl { get; set; }
    }
}
