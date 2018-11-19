using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ResourceLinkModel> ResourceLinks { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            ResourceLinks = GetResourceLinks(user);
        }

        private IList<ResourceLinkModel> GetResourceLinks(AdvantagePlatformUser user)
        {
            var list = new List<ResourceLinkModel>();

            var resourceLinks = user.ResourceLinks
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
