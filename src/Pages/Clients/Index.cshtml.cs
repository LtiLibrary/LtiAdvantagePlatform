using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public IList<ClientModel> Clients { get;set; }

        public async Task OnGetAsync()
        {
            Clients = await BuildClientListAsync();
        }

        private async Task<IList<ClientModel>> BuildClientListAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var clients = _identityContext.Clients
                .Include(client => client.Properties)
                .Where(client => client.Properties.Any(prop => prop.Value == user.Id))
                .OrderBy(client => client.ClientId);

            var list = new List<ClientModel>();
            foreach (var client in clients)
            {
                var item = new ClientModel
                {
                    Id = client.Id,
                    ClientId = client.ClientId,
                    ClientName = client.ClientName ?? client.ClientId
                };

                list.Add(item);
            }

            return list;
        }
    }
}
