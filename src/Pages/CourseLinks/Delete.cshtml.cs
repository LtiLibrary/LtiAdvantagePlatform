using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.CourseLinks
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

            var resourceLink = user.Course.ResourceLinks.SingleOrDefault(r => r.Id == id);
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
                Title = resourceLink.Title,
                ToolName = tool.Name
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var resourceLink = await _context.ResourceLinks.FindAsync(id);

            if (resourceLink != null)
            {
                _context.ResourceLinks.Remove(resourceLink);

                var gradebookColumn = await _context.GetGradebookColumnByResourceLinkAsync(id.Value);
                if (gradebookColumn != null)
                {
                    _context.GradebookColumns.Remove(gradebookColumn);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
