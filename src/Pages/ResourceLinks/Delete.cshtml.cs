using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public DeleteModel(
            ApplicationDbContext appContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
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

            var resourceLink = await _appContext.ResourceLinks.FindAsync(id);

            if (resourceLink == null || resourceLink.UserId != user.Id)
            {
                return NotFound();
            }

            var tool = await _appContext.Tools.FindAsync(resourceLink.ToolId);

            if (tool == null)
            {
                return NotFound();
            }

            ResourceLink = new ResourceLinkModel
            {
                Id = resourceLink.Id,
                Title = resourceLink.Title,
                ToolName = tool.Name,
                LinkContext = resourceLink.LinkContext
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
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
