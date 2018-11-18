using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;

        public EditModel(ApplicationDbContext appContext)
        {
            _appContext = appContext;
        }

        [BindProperty]
        public ResourceLinkModel ResourceLink { get; set; }

        public IList<SelectListItem> Tools { get; private set; }
        public IList<SelectListItem> ToolPlacements { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _appContext.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                return NotFound();
            }

            ResourceLink = new ResourceLinkModel
            {
                Id = resourceLink.Id,
                LinkContext = resourceLink.LinkContext,
                Title = resourceLink.Title,
                ToolId = resourceLink.ToolId
            };

            Tools = user.Tools
                .OrderBy(tool => tool.Name)
                .Select(tool => new SelectListItem
                {
                    Text = tool.Name,
                    Value = tool.Id.ToString()
                })
                .ToList();

            ToolPlacements = Enum.GetNames(typeof(ResourceLink.LinkContexts))
                .Select(t => new SelectListItem
                {
                    Value = t,
                    Text = t
                })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var resourceLink = await _appContext.ResourceLinks.FindAsync(ResourceLink.Id);
            resourceLink.LinkContext = ResourceLink.LinkContext;
            resourceLink.Title = ResourceLink.Title;
            resourceLink.ToolId = ResourceLink.ToolId;

            _appContext.Attach(resourceLink).State = EntityState.Modified;

            try
            {
                await _appContext.SaveChangesAsync();
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
            return _appContext.ResourceLinks.Any(e => e.Id == id);
        }
    }
}
