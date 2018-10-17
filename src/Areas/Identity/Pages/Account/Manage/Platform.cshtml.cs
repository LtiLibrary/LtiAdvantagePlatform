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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (user.PlatformId != null)
            {
                Platform = await _context.Platforms.FindAsync(user.PlatformId);
            }

            if (Platform == null)
            {
                Platform = RegisterModel.CreatePlatform(Request, user);
                await _context.Platforms.AddAsync(Platform);
                await _context.SaveChangesAsync();
                user.PlatformId = Platform.Id;
                await _userManager.UpdateAsync(user);
            }

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