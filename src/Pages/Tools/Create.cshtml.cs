using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityModel.Client;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Constants = LtiAdvantage.Constants;
using JsonWebKeySet = IdentityModel.Jwk.JsonWebKeySet;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationDbContext _identityContext;

        [BindProperty]
        public ToolModel Tool { get; set; }

        public CreateModel(
            ApplicationDbContext context, 
            IHttpClientFactory httpClientFactory,
            IConfigurationDbContext identityContext)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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

            if (Tool.JwkSetUrl.IsPresent())
            {
                var httpClient = _httpClientFactory.CreateClient();

                // Test whether JsonWebKeySetUrl points to a Discovery Document
                var disco = await httpClient.GetDiscoveryDocumentAsync(Tool.JwkSetUrl);
                if (!disco.IsError)
                {
                    Tool.JwkSetUrl = disco.JwksUri;
                }
                else
                {
                    // Test that JsonWebKeySetUrl points to a JWKS endpoint
                    try
                    {
                        var keySetJson = await httpClient.GetStringAsync(Tool.JwkSetUrl);
                        JsonConvert.DeserializeObject<JsonWebKeySet>(keySetJson);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.JwkSetUrl)}",
                            e.Message);
                    }
                }
            }

            if (Tool.JwkSetUrl.IsMissing() && Tool.PublicKey.IsMissing())
            {
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.JwkSetUrl)}",
                    "Either JWK Set URL or Public Key is required.");
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.PublicKey)}",
                    "Either JWK Set URL or Public Key is required.");
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
                    Constants.LtiScopes.AssignmentGradesLineItem, 
                    Constants.LtiScopes.AssignmentGradesResultReadonly,
                    Constants.LtiScopes.NamesRoleReadonly
                }
            };
            if (Tool.PublicKey.IsPresent())
            {
                client.ClientSecrets = new List<Secret>
                {
                    new Secret
                    {
                        Type = LtiAdvantage.IdentityServer4.Validation.Constants.SecretTypes.PublicPemKey,
                        Value = Tool.PublicKey
                    }
                };
            }

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
                JsonWebKeySetUrl = Tool.JwkSetUrl,
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