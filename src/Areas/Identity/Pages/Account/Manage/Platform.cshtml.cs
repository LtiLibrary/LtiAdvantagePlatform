using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Areas.Identity.Pages.Account.Manage
{
    public class PlatformModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public PlatformModel(
            ApplicationDbContext context,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Platform Platform { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Platform = user.Platform;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Platform).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}