using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
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
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public CreateModel(ApplicationDbContext appContext, IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        [BindProperty]
        public ResourceLink ResourceLink { get; set; }

        [BindProperty]
        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }
        public IList<SelectListItem> Clients { get; private set; }

        public IList<SelectListItem> ToolPlacements { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);

            Clients = await _identityContext.Clients
                .Include(client => client.Properties)
                .Where(client => client.Properties.Any(prop => prop.Value == user.Id))
                .OrderBy(client => client.ClientName)
                .Select(client => new SelectListItem
                {
                    Text = client.ClientName,
                    Value = client.Id.ToString()
                })
                .ToListAsync();

            ToolPlacements = Enum.GetNames(typeof(ResourceLink.ToolPlacements))
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
            ResourceLink.ClientId = ClientId;

            _appContext.ResourceLinks.Add(ResourceLink);
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}