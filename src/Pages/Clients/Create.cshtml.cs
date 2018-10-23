using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        [BindProperty]
        public ClientModel Client { get; set; }

        public CreateModel(IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            Client = new ClientModel
            {
                ClientId = Guid.NewGuid().ToString("N"),
                ClientSecret = GenerateClientSecret()
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new Client
            {
                ClientId = Client.ClientId,
                ClientName = Client.ClientName,
                ClientSecrets = { new Secret(Client.ClientSecret.Sha256()) },

                AllowOfflineAccess = true,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new [] { "api1" }
            };

            // Save the ClientSecret in plain text so Tool owner can retrieve it
            // like Google does in their developer console
            client.Properties.Add("secret", Client.ClientSecret);

            // Record the user that created this client
            var user = await _userManager.GetUserAsync(User);
            client.Properties.Add("userid", user.Id);

            var entity = client.ToEntity();

            await _identityContext.Clients.AddAsync(entity);
            await _identityContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private static string GenerateClientSecret()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var buffer = new byte[24];
                rng.GetBytes(buffer);
                return Base64UrlEncoder.Encode(buffer);
            }
        }
    }
}