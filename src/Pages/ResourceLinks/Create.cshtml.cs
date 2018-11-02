using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public CreateModel(ApplicationDbContext appContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _userManager = userManager;
        }

        [BindProperty]
        public ResourceLinkModel ResourceLink { get; set; }

        public IList<SelectListItem> Tools { get; private set; }
        public IList<SelectListItem> ToolPlacements { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            Tools = await _appContext.Tools
                .Where(tool => tool.UserId == user.Id)
                .OrderBy(tool => tool.ToolName)
                .Select(tool => new SelectListItem
                {
                    Text = tool.ToolName,
                    Value = tool.Id.ToString()
                })
                .ToListAsync();

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

            var user = await _userManager.GetUserAsync(User);

            var resourceLink = new ResourceLink
            {
                LinkContext = ResourceLink.LinkContext,
                Title = ResourceLink.Title,
                ToolId = ResourceLink.ToolId,
                UserId = user.Id
            };

            _appContext.ResourceLinks.Add(resourceLink);
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}