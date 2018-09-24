using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.NetCore.Utilities;
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

        public Platform Platform { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Platform = await _context.Platforms.SingleOrDefaultAsync(p => p.UserId == user.Id);
            if (Platform == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load platform.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Platform = await _context.Platforms.SingleOrDefaultAsync(p => p.UserId == user.Id);
            var keyPair = RsaHelper.GenerateRsaKeyPair();
            Platform.PublicKey = keyPair.PublicKey;
            Platform.PrivateKey = keyPair.PrivateKey;
            _context.Platforms.Update(Platform);
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}