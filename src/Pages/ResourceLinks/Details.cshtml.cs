using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public DetailsModel(
            ApplicationDbContext appContext, 
            IConfigurationDbContext identityContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

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
    }
}
