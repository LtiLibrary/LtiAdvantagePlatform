using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class ResourceLinkModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Tool")]
        public int ToolId { get; set; }

        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }

        [Required]
        [Display(Name = "Context")]
        public ResourceLink.LinkContexts? LinkContext { get; set; }
    }
}
