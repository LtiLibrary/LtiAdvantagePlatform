using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ResourceLinkModel ResourceLink { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            var tool = resourceLink.Tool;
            if (tool == null)
            {
                return NotFound();
            }

            ResourceLink = new ResourceLinkModel
            {
                Id = resourceLink.Id,
                CustomProperties = resourceLink.CustomProperties,
                LinkContext = user.Course.ResourceLinks.Any(l => l.Id == resourceLink.Id) ? ResourceLinkModel.LinkContexts.Course : ResourceLinkModel.LinkContexts.Platform,
                Title = resourceLink.Title,
                ToolName = tool.Name
            };

            return Page();
        }
    }
}
