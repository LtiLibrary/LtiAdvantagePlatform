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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchPageModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public LaunchPageModel(
            ApplicationDbContext appContext,
            IHttpClientFactory httpClientFactory,
            IConfigurationDbContext identityContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _httpClientFactory = httpClientFactory;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            var tool = await _appContext.Tools.FindAsync(resourceLink.ToolId);
            if (tool == null)
            {
                return NotFound();
            }

            if (tool.JsonWebKeySetUrl.IsPresent())
            {
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
                    Console.WriteLine(e);
                    throw;
                }
            }

            ViewData["Title"] = resourceLink.Title;

            return Page();
        }

        private IList<JsonWebKey> GetJsonWebKeySecrets(IList<ClientSecret> clientSecrets)
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