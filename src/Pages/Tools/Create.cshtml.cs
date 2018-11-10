using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using LtiAdvantageLibrary.NetCore;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        [BindProperty]
        public ToolModel Tool { get; set; }

        public CreateModel(
            ApplicationDbContext appContext, 
            IConfigurationDbContext identityContext, 
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            var keyPair = RsaHelper.GenerateRsaKeyPair();

            Tool = new ToolModel
            {
                DeploymentId = GenerateRandomString(8),
                PrivateKey = keyPair.PrivateKey,
                PublicKey = keyPair.PublicKey
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (_identityContext.Clients.Any(c => c.ClientId == Tool.ClientId))
            {
                ModelState.AddModelError("Tool.ClientId", "This Client ID already exists.");
                return Page();
            }

            var client = new Client
            {
                ClientId = Tool.ClientId,
                ClientName = Tool.Name,

                AllowOfflineAccess = true,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { Constants.LtiScopes.MembershipReadonly }
            };

            // Add all the secrets
            client.ClientSecrets = new List<Secret>
            {
                new Secret
                {
                    Type = Constants.SecretTypes.PublicKey,
                    Value = Tool.PublicKey
                },
                new Secret
                {
                    Type = Constants.SecretTypes.PrivateKey,
                    Value = Tool.PrivateKey
                }
            };

            if (Tool.ClientSecret.IsPresent())
            {
                client.ClientSecrets.Add(new Secret(Tool.ClientSecret.Sha256()));
                client.Properties = new Dictionary<string, string> 
                    {{ IdentityServerConstants.SecretTypes.SharedSecret, Tool.ClientSecret }};
            }

            var entity = client.ToEntity();

            await _identityContext.Clients.AddAsync(entity);
            await _identityContext.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            var tool = new Tool
            {
                DeploymentId = Tool.DeploymentId,
                IdentityServerClientId = entity.Id,
                Name = Tool.Name,
                Url = Tool.Url,
                UserId = user.Id
            };

            await _appContext.Tools.AddAsync(tool);
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        private static string GenerateRandomString(int length = 24)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[length];
                rng.GetBytes(buffer);
                return Base64UrlEncoder.Encode(buffer);
            }
        }
    }
}