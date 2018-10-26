using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(
            ApplicationDbContext appContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        public IList<ResourceLinkModel> ResourceLinks { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            ResourceLinks = await GetDeplomentModelsAsync(user.Id);
        }

        private async Task<IList<ResourceLinkModel>> GetDeplomentModelsAsync(string userId)
        {
            var list = new List<ResourceLinkModel>();

            var resourceLinks = _appContext.ResourceLinks
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Title);

            foreach (var link in resourceLinks)
            {
                var tool = await _appContext.Tools.FindAsync(link.ToolId);

                list.Add(new ResourceLinkModel
                {
                    Id = link.Id,
                    Title = link.Title,
                    ToolName = tool.Name,
                    LinkContext = link.LinkContext
                });
            }

            return list;
        }
    }
}
