using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.PlatformLinks
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResourceLinkModel ResourceLink { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var user = await _context.GetUserFullAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var resourceLink = user.Platform.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            ResourceLink = new ResourceLinkModel(resourceLink);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var resourceLink = await _context.ResourceLinks.FindAsync(id);

            if (resourceLink != null)
            {
                _context.ResourceLinks.Remove(resourceLink);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
