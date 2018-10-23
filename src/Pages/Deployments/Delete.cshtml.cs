using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Deployments
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public DeleteModel(
            ApplicationDbContext appContext, 
            IConfigurationDbContext identityContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        [BindProperty]
        public DeploymentModel Deployment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var deployment = await _appContext.Deployments
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (deployment == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients.FindAsync(deployment.ClientId);

            Deployment = new DeploymentModel
            {
                Id = deployment.Id,
                ClientName = client?.ClientName,
                ToolName = deployment.ToolName,
                ToolPlacement = deployment.ToolPlacement,
                ToolUrl = deployment.ToolUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deployment = await _appContext.Deployments.FindAsync(id);

            if (deployment != null)
            {
                _appContext.Deployments.Remove(deployment);
                await _appContext.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
