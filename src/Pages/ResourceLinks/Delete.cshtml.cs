using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.ResourceLinks
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
        public ResourceLinkModel ResourceLink { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            var resourceLink = await _appContext.ResourceLinks
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (resourceLink == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients.FindAsync(resourceLink.ClientId);

            ResourceLink = new ResourceLinkModel
            {
                Id = resourceLink.Id,
                ClientName = client?.ClientName,
                DeploymentId = resourceLink.DeploymentId,
                ToolName = resourceLink.ToolName,
                ToolPlacement = resourceLink.ToolPlacement,
                ToolUrl = resourceLink.ToolUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resourceLink = await _appContext.ResourceLinks.FindAsync(id);

            if (resourceLink != null)
            {
                _appContext.ResourceLinks.Remove(resourceLink);
                await _appContext.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
