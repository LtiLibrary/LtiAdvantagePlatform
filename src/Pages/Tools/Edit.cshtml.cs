using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage.IdentityServer4;
using LtiAdvantage.IdentityServer4.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Tools
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;

        public EditModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext)
        {
            _context = context;
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var tool = await _context.Tools.FindAsync(Tool.Id);
            tool.CustomProperties = Tool.CustomProperties;
            tool.LaunchUrl = Tool.LaunchUrl;
            tool.Name = Tool.Name;

            _context.Tools.Attach(tool).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);

            client.ClientId = Tool.ClientId;

            var privateKey = client.ClientSecrets
                .SingleOrDefault(s => s.Type == Constants.SecretTypes.PrivatePemKey);

            if (Tool.PrivateKey.IsPresent())
            {
                if (privateKey == null)
                {
                    privateKey = new ClientSecret
                    {
                        Client = client,
                        Type = Constants.SecretTypes.PrivatePemKey
                    };
                    client.ClientSecrets.Add(privateKey);
                }
                privateKey.Value = Tool.PrivateKey;
            }
            else
            {
                if (privateKey != null)
                {
                    client.ClientSecrets.Remove(privateKey);
                }
            }

            _identityContext.Clients.Update(client);
            await _identityContext.SaveChangesAsync();


            return RedirectToPage("./Index");
        }
    }
}
