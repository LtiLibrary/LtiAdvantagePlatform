using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using ClientSecret = AdvantagePlatform.Data.ClientSecret;

namespace AdvantagePlatform.Pages.Clients
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public DetailsModel(ApplicationDbContext appContext, IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public ClientModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var clientSecret = await _appContext.ClientSecretText
                    .Where(secret => user.ClientIds.Contains(secret.ClientId))
                    .SingleOrDefaultAsync(secret => secret.ClientId == id);
            if (clientSecret == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients
                .Where(c => user.ClientIds.Contains(c.Id))
                .SingleOrDefaultAsync(c => c.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            Client = new ClientModel
            {
                Id = client.Id,
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                ClientSecret = clientSecret.Secret 
            };

            return Page();
        }

        public class ClientModel
        {
            public int Id { get; set; }

            [Display(Name = "Client ID")]
            public string ClientId { get; set; }

            [Display(Name = "Name")]
            public string ClientName { get; set; }

            [Display(Name = "Client Secret")]
            public string ClientSecret { get; set; }
        }
    }
}
