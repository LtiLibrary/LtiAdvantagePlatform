using System.Collections.Generic;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.PlatformLinks
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
            var user = await _context.GetUserFullAsync(User);
            if (user == null)
            {
                return;
            }

            ResourceLinks = ResourceLinkModel.GetResourceLinks(user.Platform.ResourceLinks);
        }
    }
}
