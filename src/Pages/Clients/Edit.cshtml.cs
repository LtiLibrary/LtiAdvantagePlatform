using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Client Client { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            Client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (Client == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(Client.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostRegenerateAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Client).State = EntityState.Modified;

            var keypair = RsaHelper.GenerateRsaKeyPair();
            Client.PrivateKey = keypair.PrivateKey;
            Client.PublicKey = keypair.PublicKey;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(Client.Id))
                {
                    return NotFound();
                }

                throw;
            }
            return Page();
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
