using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
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

            tool.ToolName = Tool.ToolName;
            tool.ToolUrl = Tool.ToolUrl;

            _appContext.Tools.Attach(tool).State = EntityState.Modified;
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
