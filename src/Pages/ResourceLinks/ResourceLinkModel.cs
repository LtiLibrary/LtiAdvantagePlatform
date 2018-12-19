using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class ResourceLinkModel
    {
        public ResourceLinkModel() {}

        public ResourceLinkModel(ResourceLink resourceLink)
        {
            Id = resourceLink.Id;
            CustomProperties = resourceLink.CustomProperties;
            Description = resourceLink.Description;
            Title = resourceLink.Title;
            ToolId = resourceLink.Tool.Id;
            ToolName = resourceLink.Tool.Name;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        [Display(Name = "Custom Properties", Description = "<p>Custom properties to include in all tool launches.<p><p>Put each name=value pair on a separate line.</p>")]
        public string CustomProperties { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Tool")]
        public int ToolId { get; set; }

        [Display(Name = "Tool Name")]
        public string ToolName { get; set; }

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
                list.Add(new ResourceLinkModel
                {
                    Id = link.Id,
                    Title = link.Title,
                    Description = link.Description,
                    ToolName = tool.Name,
                    CustomProperties = link.CustomProperties
                });
            }

            return list;
        }

        public ResourceLink ToResourceLink(Tool tool)
        {
            return new ResourceLink
            {
                Id = Id,
                CustomProperties = CustomProperties,
                Description = Description,
                Title = Title,
                Tool = tool
            };
        }

        public void UpdateResourceLink(ResourceLink resourceLink, Tool tool)
        {
            resourceLink.CustomProperties = CustomProperties;
            resourceLink.Description = Description;
            resourceLink.Title = Title;
            resourceLink.Tool = tool;
        }
    }
}
