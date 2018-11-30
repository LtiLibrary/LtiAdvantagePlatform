using System.Collections.Generic;
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

        [Display(Name = "Custom Properties", Description = "<p>Custom properties to include in all tool launches.<p><p>Put each name=value pair on a separate line.</p>")]
        public string CustomProperties { get; set; }

        /// <summary>
        /// Convert the resource links into ResourceLinkModels.
        /// </summary>
        /// <param name="resourceLinks">The resource links to convert.</param>
        /// <returns></returns>
        public static IList<ResourceLinkModel> GetResourceLinks(ICollection<ResourceLink> resourceLinks)
        {
            var list = new List<ResourceLinkModel>();

            foreach (var link in resourceLinks)
            {
                var tool = link.Tool;
                if (tool == null)
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        Title = link.Title,
                        CustomProperties = link.CustomProperties
                    });
                }
                else
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        Title = link.Title,
                        ToolName = tool.Name,
                        CustomProperties = link.CustomProperties
                    });
                }
            }

            return list;
        }
    }
}
