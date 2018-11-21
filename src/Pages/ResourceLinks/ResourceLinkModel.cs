using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        /// <summary>
        /// Get the user's resource links.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IList<ResourceLinkModel> GetResourceLinks(AdvantagePlatformUser user, ResourceLink.LinkContexts? context = null)
        {
            var list = new List<ResourceLinkModel>();


            var resourceLinks = context.HasValue
                ? user.ResourceLinks
                    .Where(r => r.LinkContext == context)
                    .OrderBy(d => d.Title)
                : user.ResourceLinks
                    .OrderBy(d => d.Title);

            foreach (var link in resourceLinks)
            {
                var tool = link.Tool;

                if (tool == null)
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        Title = link.Title,
                        LinkContext = link.LinkContext
                    });
                }
                else
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        Title = link.Title,
                        ToolName = tool.Name,
                        LinkContext = link.LinkContext
                    });
                }
            }

            return list;
        }

    }
}
