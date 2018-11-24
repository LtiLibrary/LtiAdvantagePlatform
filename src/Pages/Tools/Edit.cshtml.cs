using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityModel.Client;
using IdentityModel.Jwk;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AdvantagePlatform.Pages.Tools
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationDbContext _identityContext;

        public EditModel(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfigurationDbContext identityContext)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _identityContext = identityContext;
        }

        [BindProperty]
        public ToolModel Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var tool = user.Tools.SingleOrDefault(t => t.Id == id);
            if (tool == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);
            if (client == null)
            {
                return NotFound();
            }

            Tool = new ToolModel(tool, client);

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
                        "Cannot parse the Custom Properties.");
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
                        return Page();
                    }
                }
            }

            if (Tool.JwkSetUrl.IsMissing() && Tool.PublicKey.IsMissing())
            {
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.JwkSetUrl)}",
                    "Either JWK Set URL or Public Key is required.");
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.PublicKey)}",
                    "Either JWK Set URL or Public Key is required.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var tool = await _context.Tools.FindAsync(Tool.Id);
            tool.CustomProperties = Tool.CustomProperties;
            tool.LaunchUrl = Tool.LaunchUrl;
            tool.JsonWebKeySetUrl = Tool.JwkSetUrl;
            tool.Name = Tool.Name;

            _context.Tools.Attach(tool).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);

            client.ClientId = Tool.ClientId;

            var publicKey = client.ClientSecrets
                .SingleOrDefault(s => s.Type == Constants.SecretTypes.PublicPemKey);

            if (Tool.PublicKey.IsPresent())
            {
                if (publicKey == null)
                {
                    publicKey = new ClientSecret
                    {
                        Client = client,
                        Type = Constants.SecretTypes.PublicPemKey
                    };
                    client.ClientSecrets.Add(publicKey);
                }
                publicKey.Value = Tool.PublicKey;
            }
            else
            {
                if (publicKey != null)
                {
                    client.ClientSecrets.Remove(publicKey);
                }
            }

            _identityContext.Clients.Update(client);
            await _identityContext.SaveChangesAsync();


            return RedirectToPage("./Index");
        }
    }
}
