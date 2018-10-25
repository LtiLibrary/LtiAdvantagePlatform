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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public EditModel(ApplicationDbContext appContext, IConfigurationDbContext identityContext, UserManager<AdvantagePlatformUser> userManager)
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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);

            ResourceLink = await _appContext.ResourceLinks
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (ResourceLink == null)
            {
                return NotFound();
            }

            ClientId = ResourceLink.ClientId;
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

            ResourceLink.ClientId = ClientId;

            _appContext.Attach(ResourceLink).State = EntityState.Modified;

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
