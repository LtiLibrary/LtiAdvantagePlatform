﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using IdentityModel;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityConfig;

        [BindProperty]
        public ToolModel Tool { get; set; }

        public CreateModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityConfig)
        {
            _context = context;
            _identityConfig = identityConfig;
        }

        public IActionResult OnGet()
        {
            // Create the Client for this tool registration
            Tool = new ToolModel(Request.HttpContext)
            {
                ClientId = CryptoRandom.CreateUniqueId(8),
                DeploymentId = CryptoRandom.CreateUniqueId(8)
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

            if (_identityConfig.Clients.Any(c => c.ClientId == Tool.ClientId))
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
                AllowedGrantTypes = GrantTypes.ImplicitAndClientCredentials, 
                AllowedScopes = Config.LtiScopes,
                ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Type = LtiAdvantage.IdentityServer4.Validation.Constants.SecretTypes.PublicPemKey,
                        Value = Tool.PublicKey
                    }
                },
                RedirectUris = { Tool.LaunchUrl },
                RequireConsent = false
            };

            // Create the IdentityServer Client first to get its primary key
            var entity = client.ToEntity();
            await _identityConfig.Clients.AddAsync(entity);
            await _identityConfig.SaveChangesAsync();

            var tool = new Tool
            {
                CustomProperties = Tool.CustomProperties,
                DeepLinkingLaunchUrl = Tool.DeepLinkingLaunchUrl,
                DeploymentId = Tool.DeploymentId,
                IdentityServerClientId = entity.Id,
                Name = Tool.Name,
                LaunchUrl = Tool.LaunchUrl,
                LoginUrl = Tool.LoginUrl
            };
            await _context.Tools.AddAsync(tool);

            var user = await _context.GetUserLightAsync(User);
            user.Tools.Add(tool);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}