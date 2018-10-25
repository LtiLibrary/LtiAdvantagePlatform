using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(
            ApplicationDbContext appContext, 
            IConfigurationDbContext identityContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
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
                .OrderBy(d => d.ClientId);

            Client client = null;
            foreach (var resourceLink in resourceLinks)
            {
                if (client == null || client.Id != resourceLink.ClientId)
                {
                    client = await _identityContext.Clients.FindAsync(resourceLink.ClientId);
                }

                list.Add(new ResourceLinkModel
                {
                    Id = resourceLink.Id,
                    ToolName = resourceLink.ToolName,
                    ToolPlacement = resourceLink.ToolPlacement,
                    ToolUrl = resourceLink.ToolUrl,
                    ClientName = client == null ? "[No Client]" : client.ClientName
                });
            }

            list = list.OrderBy(d => d.ToolName).ToList();

            return list;
        }
    }
}
