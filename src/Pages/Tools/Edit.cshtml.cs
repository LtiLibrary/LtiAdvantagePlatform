using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Secret = IdentityServer4.Models.Secret;

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

            var client = await _identityContext.Clients.FindAsync(tool.IdentSvrClientId);

            if (client == null)
            {
                return NotFound();
            }

            Tool = new ToolModel
            {
                Id = tool.Id,
                ToolClientId = client.ClientId,
                DeploymentId = tool.DeploymentId,
                ToolIssuer = tool.ToolIssuer,
                ToolJsonWebKeysUrl = tool.ToolJsonWebKeysUrl,
                ToolName = tool.ToolName,
                ToolUrl = tool.ToolUrl
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

            tool.ToolIssuer = Tool.ToolIssuer;
            tool.ToolJsonWebKeysUrl = Tool.ToolJsonWebKeysUrl;
            tool.ToolName = Tool.ToolName;
            tool.ToolUrl = Tool.ToolUrl;

            _appContext.Tools.Attach(tool).State = EntityState.Modified;
            await _appContext.SaveChangesAsync();

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentSvrClientId);

            if (!string.IsNullOrEmpty(Tool.ToolClientSecret))
            {
                client.ClientSecrets.Clear();
                client.ClientSecrets.Add(new ClientSecret { Client = client, Value = Tool.ToolClientSecret.Sha256() });
            }

            _identityContext.Clients.Update(client);
            await _identityContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
