using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class ResourceLinkModel
    {
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }

        [Display(Name = "Context")]
        public ResourceLink.LinkContexts? LinkContext { get; set; }
    }
}
