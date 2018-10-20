using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public string ClientId { get; set; }

        [BindProperty]
        public MyClient MyClient { get; set; }

        public CreateModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            ClientId = Guid.NewGuid().ToString("N");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Add the user ID
            var user = await _userManager.GetUserAsync(User);
            MyClient.UserId = user.Id;

            await _context.MyClients.AddAsync(MyClient);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}