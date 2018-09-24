using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public string ClientId { get; set; }
        public string CreatorId { get; set; }

        [BindProperty]
        public Client Client { get; set; }

        public CreateModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
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

            // Add the public and private keys
            var keypair = RsaHelper.GenerateRsaKeyPair();
            Client.PrivateKey = keypair.PrivateKey;
            Client.PublicKey = keypair.PublicKey;

            _context.Clients.Add(Client);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}