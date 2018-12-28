using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AdvantagePlatform.Pages.CourseLinks
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResourceLinkModel ResourceLink { get; set; }
        public IList<SelectListItem> Tools { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _context.GetUserLightAsync(User);
            if (user == null)
            {
                return NotFound();
            }

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

            var user = await _context.GetUserFullAsync(User);
            var tool = await _context.Tools.FindAsync(ResourceLink.ToolId);
            
            // Add the resource link to the course
            var resourceLink = ResourceLink.ToResourceLink(tool);
            user.Course.ResourceLinks.Add(resourceLink);

            // And add a gradebook column to the course
            var gradebookColumn = new GradebookColumn
            {
                EndDateTime = DateTime.UtcNow.AddMonths(3),
                Label = ResourceLink.Title,
                ResourceLink = resourceLink,
                ScoreMaximum = 100,
                StartDateTime = DateTime.UtcNow,
                Tag = tool.Name
            };
            user.Course.GradebookColumns.Add(gradebookColumn);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}