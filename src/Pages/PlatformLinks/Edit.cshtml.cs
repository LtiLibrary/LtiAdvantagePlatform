using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using AdvantagePlatform.Utility;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.PlatformLinks
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResourceLinkModel ResourceLink { get; set; }
        public IList<SelectListItem> Tools { get; private set; }

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

            Tools = user.Tools
                .OrderBy(tool => tool.Name)
                .Select(tool => new SelectListItem
                {
                    Text = tool.Name,
                    Value = tool.Id.ToString()
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ResourceLink.CustomProperties.IsPresent())
            {
                if (!ResourceLink.CustomProperties.TryConvertToDictionary(out _))
                {
                    ModelState.AddModelError(
                        $"{nameof(Tool)}.{nameof(Tool.CustomProperties)}",
                        "Cannot parse the Custom Properties.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resourceLink = await _context.ResourceLinks.FindAsync(ResourceLink.Id);
            var tool = await _context.Tools.FindAsync(ResourceLink.ToolId);

            ResourceLink.UpdateResourceLink(resourceLink, tool);

            _context.Attach(resourceLink).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceLinkExists(ResourceLink.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool ResourceLinkExists(int id)
        {
            return _context.ResourceLinks.Any(e => e.Id == id);
        }
    }
}
