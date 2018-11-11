using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchPageModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public LaunchPageModel(
            ApplicationDbContext appContext,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

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

            ViewData["Title"] = resourceLink.Title;

            return Page();
        }
    }
}