using System;
using System.ComponentModel.DataAnnotations;
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
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        [BindProperty]
        public ClientModel Client { get; set; }

        public CreateModel(ApplicationDbContext appContext, IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
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

            var entity = client.ToEntity();

            await _identityContext.Clients.AddAsync(entity);
            await _identityContext.SaveChangesAsync();

            // Save the ClientSecret in plain text so Tool owner can retrieve it
            // like Google does in their developer console
            var clientSecret = new ClientSecret
            {
                ClientId = entity.Id,
                Secret = Client.ClientSecret
            };
            await _appContext.ClientSecretText.AddAsync(clientSecret);
            await _appContext.SaveChangesAsync();

            // Associate this client with the user that created it
            var user = await _userManager.GetUserAsync(User);
            user.ClientIds.Add(entity.Id);
            await _userManager.UpdateAsync(user);

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