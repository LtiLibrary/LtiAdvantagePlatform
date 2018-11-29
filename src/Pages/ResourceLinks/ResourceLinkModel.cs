using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class ResourceLinkModel
    {
        public enum LinkContexts
        {
            Course,
            Platform
        }

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
        public LinkContexts LinkContext { get; set; }

        [Display(Name = "Custom Properties", Description = "<p>Custom properties to include in all tool launches.<p><p>Put each name=value pair on a separate line.</p>")]
        public string CustomProperties { get; set; }

        /// <summary>
        /// Convert the resource links into ResourceLinkModels.
        /// </summary>
        /// <param name="resourceLinks">The resource links to convert.</param>
        /// <returns></returns>
        public static IList<ResourceLinkModel> GetResourceLinks(AdvantagePlatformUser user, LinkContexts? linkContextFilter)
        {
            var list = new List<ResourceLinkModel>();

            var resourceLinks = linkContextFilter.HasValue
                ? linkContextFilter == LinkContexts.Course ? user.Course.ResourceLinks : user.Platform.ResourceLinks
                : user.ResourceLinks;

            foreach (var link in resourceLinks)
            {
                var tool = link.Tool;
                LinkContexts linkContext;

                if (linkContextFilter.HasValue)
                {
                    linkContext = linkContextFilter.Value;
                }
                else
                {
                    if (user.Course.ResourceLinks.Any(l => l.Id == link.Id))
                    {
                        linkContext = LinkContexts.Course;
                    }
                    else
                    {
                        linkContext = LinkContexts.Platform;
                    }
                }

                if (tool == null)
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        LinkContext =  linkContext,
                        Title = link.Title,
                        CustomProperties = link.CustomProperties
                    });
                }
                else
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        LinkContext =  linkContext,
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
