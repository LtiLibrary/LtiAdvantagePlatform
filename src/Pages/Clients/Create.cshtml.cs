using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;

namespace AdvantagePlatform.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public string ClientId { get; set; }
        public string CreatorId { get; set; }

        [BindProperty]
        public Client Client { get; set; }

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet()
        {
            ClientId = Guid.NewGuid().ToString("N");

            var user = await _userManager.GetUserAsync(User);
            CreatorId = user.Id;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The name must be unique
            if (_context.Clients.Any(c => c.Name == Client.Name && c.CreatorId == Client.CreatorId))
            {
                ModelState.AddModelError("Client.Name", "That name is already in use.");
                return Page();
            }

            _context.Clients.Add(Client);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}