using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityModel.Jwk;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LaunchPageModel> _logger;

        public LaunchPageModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext,
            IHttpClientFactory httpClientFactory,
            ILogger<LaunchPageModel> logger
            )
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _identityContext = identityContext;
            _logger = logger;
        }

        /// <summary>
        /// IdentityServer needs the ClientSecrets to be pre-loaded when a request
        /// to authorize comes in. This method calls the client's JWKS URL to get
        /// the latest public keys.
        /// </summary>
        /// <param name="id">The <see cref="ResourceLink"/> id.</param>
        /// <returns></returns>
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

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            var tool = resourceLink.Tool;
            if (tool == null)
            {
                return NotFound();
            }

            if (tool.JsonWebKeySetUrl.IsPresent())
            {
                _logger.LogInformation($"Retrieving JWK Set from {tool.JsonWebKeySetUrl}");

                var httpClient = _httpClientFactory.CreateClient();
                try
                {
                    var keySetJson = await httpClient.GetStringAsync(tool.JsonWebKeySetUrl);
                    var keySet = JsonConvert.DeserializeObject<JsonWebKeySet>(keySetJson);
                    if (keySet.Keys.Any())
                    {
                        var client = await _identityContext.Clients
                            .Include(c => c.ClientSecrets)
                            .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);
                        if (client == null)
                        {
                            return NotFound("Client not found.");
                        }

                        var jsonWebKeys = GetJsonWebKeySecrets(client.ClientSecrets);
                        foreach (var jsonWebKey in keySet.Keys)
                        {
                            if (jsonWebKeys.All(key => key.Kid != jsonWebKey.Kid))
                            {
                                var secret = new ClientSecret
                                {
                                    Client = client,
                                    Type = Constants.SecretTypes.PublicJsonWebKey,
                                    Value = JsonConvert.SerializeObject(jsonWebKey)
                                };
                                client.ClientSecrets.Add(secret);
                            }
                        }

                        _identityContext.Clients.Update(client);
                        await _identityContext.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Cannot retrieve JWK Set from {tool.JsonWebKeySetUrl}");
                }
            }

            ViewData["Title"] = resourceLink.Title;

            return Page();
        }

        private static IList<JsonWebKey> GetJsonWebKeySecrets(IEnumerable<ClientSecret> clientSecrets)
        {
            var list = new List<JsonWebKey>();
            var secrets = clientSecrets.Where(s => s.Type == Constants.SecretTypes.PublicJsonWebKey).ToList();
            foreach (var secret in secrets)
            {
                var jsonWebKey = JsonConvert.DeserializeObject<JsonWebKey>(secret.Value);
                list.Add(jsonWebKey);
            }

            return list;
        }
    }
}