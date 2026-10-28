using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Models
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
            DocumentTarget = resourceLink.DocumentTarget;
            IframeHeight = resourceLink.IframeHeight;
            IsProctored = resourceLink.IsProctored;
            ProctoringSettings = resourceLink.ProctoringSettings;
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
        /// "iframe" to embed the tool in the page, otherwise a new window.
        /// </summary>
        public string DocumentTarget { get; set; }

        public int? IframeHeight { get; set; }

        [Display(Name = "Proctored", Description = "Launch this assessment through a proctoring tool (LTI Proctoring Services): the tool checks the student before the assessment starts.")]
        public bool IsProctored { get; set; }

        [Display(Name = "Proctoring Settings", Description = "Optional settings for the proctoring tool, sent as the proctoring_settings claim.")]
        public string ProctoringSettings { get; set; }

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
                    CustomProperties = link.CustomProperties,
                    DocumentTarget = link.DocumentTarget,
                    IframeHeight = link.IframeHeight,
                    IsProctored = link.IsProctored,
                    ProctoringSettings = link.ProctoringSettings
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
                Tool = tool,
                IsProctored = IsProctored,
                ProctoringSettings = ProctoringSettings
            };
        }

        public void UpdateResourceLink(ResourceLink resourceLink, Tool tool)
        {
            resourceLink.CustomProperties = CustomProperties;
            resourceLink.Description = Description;
            resourceLink.Title = Title;
            resourceLink.Tool = tool;
            resourceLink.IsProctored = IsProctored;
            resourceLink.ProctoringSettings = ProctoringSettings;
        }
    }
}
