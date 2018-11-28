using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Constants = LtiAdvantage.Constants;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;

        [BindProperty]
        public ToolModel Tool { get; set; }

        public CreateModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public IActionResult OnGet()
        {
            Tool = new ToolModel
            {
                DeploymentId = GenerateRandomString(8)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Tool.CustomProperties.IsPresent())
            {
                if (!Tool.CustomProperties.TryConvertToDictionary(out _))
                {
                    ModelState.AddModelError(
                        $"{nameof(Tool)}.{nameof(Tool.CustomProperties)}",
                        "Cannot parse the Custom Properites.");
                }
            }

            if (_identityContext.Clients.Any(c => c.ClientId == Tool.ClientId))
            {
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.ClientId)}",
                    "This Client ID already exists.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new Client
            {
                ClientId = Tool.ClientId,
                ClientName = Tool.Name,
                AllowOfflineAccess = true,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes =
                {
                    Constants.LtiScopes.AgsLineItem,
                    Constants.LtiScopes.AgsResultReadonly,
                    Constants.LtiScopes.NrpsMembershipReadonly
                },
                ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Type = LtiAdvantage.IdentityServer4.Validation.Constants.SecretTypes.PrivatePemKey,
                        Value = Tool.PrivateKey
                    }
                }
            };

            var entity = client.ToEntity();

            // Create the IdentityServer Client first to get its ID
            await _identityContext.Clients.AddAsync(entity);
            await _identityContext.SaveChangesAsync();

            var user = await _context.GetUserAsync(User);
            var tool = new Tool
            {
                CustomProperties = Tool.CustomProperties,
                DeploymentId = Tool.DeploymentId,
                IdentityServerClientId = entity.Id,
                Name = Tool.Name,
                LaunchUrl = Tool.LaunchUrl,
                User = user
            };

            await _context.Tools.AddAsync(tool);
            await _context.SaveChangesAsync();

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