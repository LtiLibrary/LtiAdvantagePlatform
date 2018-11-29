using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.ResourceLinks
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
        public IList<SelectListItem> ToolPlacements { get; private set; }

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

            ResourceLink = new ResourceLinkModel
            {
                Id = resourceLink.Id,
                CustomProperties = resourceLink.CustomProperties,
                LinkContext = user.Course.ResourceLinks.Any(l => l.Id == resourceLink.Id) ? ResourceLinkModel.LinkContexts.Course : ResourceLinkModel.LinkContexts.Platform,
                Title = resourceLink.Title,
                ToolId = resourceLink.Tool.Id
            };

            Tools = user.Tools
                .OrderBy(tool => tool.Name)
                .Select(tool => new SelectListItem
                {
                    Text = tool.Name,
                    Value = tool.Id.ToString()
                })
                .ToList();

            ToolPlacements = Enum.GetNames(typeof(ResourceLinkModel.LinkContexts))
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
            resourceLink.CustomProperties = ResourceLink.CustomProperties;
            resourceLink.Title = ResourceLink.Title;
            resourceLink.Tool = tool;

            // If the resourceLink changed, then move it to the right context
            var user = await _context.GetUserAsync(User);
            var oldCourse = user.Course.ResourceLinks.Any(l => l.Id == resourceLink.Id) ? user.Course : null;
            var oldPlatform = user.Platform.ResourceLinks.Any(l => l.Id == resourceLink.Id) ? user.Platform : null;
            var newCourse = ResourceLink.LinkContext == ResourceLinkModel.LinkContexts.Course ? user.Course : null;
            var newPlatform = ResourceLink.LinkContext == ResourceLinkModel.LinkContexts.Platform ? user.Platform : null;
            if (newCourse != oldCourse && oldPlatform != null && newCourse != null)
            {
                oldPlatform.ResourceLinks.Remove(resourceLink);
                newCourse.ResourceLinks.Add(resourceLink);
            }

            if (newPlatform != oldPlatform && oldCourse != null && newPlatform != null)
            {
                oldCourse.ResourceLinks.Remove(resourceLink);
                newPlatform.ResourceLinks.Add(resourceLink);
            }

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
