using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Tool Tool { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            Tool.UserId = user.Id;

            _context.Tools.Add(Tool);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}