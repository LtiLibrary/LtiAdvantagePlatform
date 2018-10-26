using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public ResourceLink ResourceLink { get; set; }

        [BindProperty]
        [Required]
        [Display(Name = "Tool")]
        public int ToolId { get; set; }
        public IList<SelectListItem> Tools { get; private set; }

        public IList<SelectListItem> ToolPlacements { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            Tools = await _appContext.Tools
                .Where(tool => tool.UserId == user.Id)
                .OrderBy(tool => tool.Name)
                .Select(tool => new SelectListItem
                {
                    Text = tool.Name,
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

            ResourceLink.UserId = user.Id;
            ResourceLink.ToolId = ToolId;

            _appContext.ResourceLinks.Add(ResourceLink);
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}