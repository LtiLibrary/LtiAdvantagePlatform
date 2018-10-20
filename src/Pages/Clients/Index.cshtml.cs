using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly IConfigurationDbContext _configuration;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(IConfigurationDbContext configuration, UserManager<AdvantagePlatformUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public IList<ClientViewModel> Clients { get;set; }

        public async Task OnGetAsync()
        {
            Clients = await BuildViewModelAsync();
        }

        private async Task<IList<ClientViewModel>> BuildViewModelAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var clients = _configuration.Clients
                .Where(client => user.ClientIds.Contains(client.Id));

            var list = new List<ClientViewModel>();
            foreach (var client in clients)
            {
                var item = new ClientViewModel
                {
                    Id = client.Id,
                    ClientId = client.ClientId,
                    ClientName = client.ClientName ?? client.ClientId
                };

                list.Add(item);
            }

            return list;
        }

        public class ClientViewModel
        {
            public int Id { get; set; }

            [Display(Name = "Client ID")]
            public string ClientId { get; set; }

            [Display(Name = "Display Name")]
            public string ClientName { get; set; }
        }
    }
}
