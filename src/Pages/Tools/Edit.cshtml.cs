using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Models;
using LtiAdvantageLibrary.NetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Tools
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public EditModel(
            ApplicationDbContext appContext,
            IConfigurationDbContext identityContext, 
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        [BindProperty]
        public ToolModel Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var tool = await _appContext.Tools.FindAsync(id);
            if (tool == null || tool.UserId != user.Id)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .Include(c => c.Properties)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);
            if (client == null)
            {
                return NotFound();
            }

            Tool = new ToolModel
            {
                Id = tool.Id,
                ClientId = client.ClientId,
                ClientSecret = client.Properties
                    .FirstOrDefault(c => c.Key == IdentityServerConstants.SecretTypes.SharedSecret)
                    ?.Value,
                DeploymentId = tool.DeploymentId,
                Name = tool.Name,
                PrivateKey = client.ClientSecrets
                    .FirstOrDefault(c => c.Type == Constants.SecretTypes.PrivateKey)
                    ?.Value,
                PublicKey = client.ClientSecrets
                    .FirstOrDefault(c => c.Type == Constants.SecretTypes.PublicKey)
                    ?.Value,
                Url = tool.Url
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var tool = await _appContext.Tools.FindAsync(Tool.Id);

            tool.Name = Tool.Name;
            tool.Url = Tool.Url;

            _appContext.Tools.Attach(tool).State = EntityState.Modified;
            await _appContext.SaveChangesAsync();

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .Include(c => c.Properties)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);

            var clearSecret =
                client.Properties.SingleOrDefault(p => p.Key == IdentityServerConstants.SecretTypes.SharedSecret);
            var sharedSecret =
                client.ClientSecrets.SingleOrDefault(s => s.Type == IdentityServerConstants.SecretTypes.SharedSecret);

            if (Tool.ClientSecret.IsPresent())
            {
                // This is not needed for IdentityServer. It is only here so that
                // testing is made easier by being able to display the current secret.
                if (clearSecret == null)
                {
                    clearSecret = new ClientProperty { Client = client };
                    client.Properties.Add(clearSecret);
                }

                clearSecret.Key = IdentityServerConstants.SecretTypes.SharedSecret;
                clearSecret.Value = Tool.ClientSecret;

                if (sharedSecret == null)
                {
                    sharedSecret = new ClientSecret { Client = client };
                    client.ClientSecrets.Add(sharedSecret);
                }

                sharedSecret.Type = IdentityServerConstants.SecretTypes.SharedSecret;
                sharedSecret.Value = Tool.ClientSecret.Sha256();
            }
            else
            {
                if (clearSecret != null)
                {
                    client.Properties.Remove(clearSecret);
                }

                if (sharedSecret != null)
                {
                    client.ClientSecrets.Remove(sharedSecret);
                }
            }

            _identityContext.Clients.Update(client);
            await _identityContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
