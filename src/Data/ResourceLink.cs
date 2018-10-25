using System.ComponentModel.DataAnnotations;

namespace AdvantagePlatform.Data
{
    public class ResourceLink
    {
        public int Id { get; set; }

        /// <summary>
        /// The Client to use with this ResourceLink.
        /// </summary>
        public int ClientId { get; set; }

        [Required]
        [Display(Name = "Deployment ID")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// The Tool name.
        /// </summary>
        [Required]
        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }

        /// <summary>
        /// The Tool placement of this ResourceLink. Can be either the user's
        /// platform or the user's course.
        /// </summary>
        [Required]
        [Display(Name = "Tool Placement")]
        public ToolPlacements? ToolPlacement { get; set; }

        /// <summary>
        /// The URL used to launch the Tool.
        /// </summary>
        [Url]
        [Required]
        [Display(Name = "Tool URL")]
        public string ToolUrl { get; set; }

        /// <summary>
        /// The ID of the AdvantagePlatformUser that created this ResourceLink.
        /// </summary>
        public string UserId { get; set; }

        public enum ToolPlacements
        {
            Course,
            Platform
        }
    }
}
