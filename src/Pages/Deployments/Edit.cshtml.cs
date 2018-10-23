using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Deployments
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
        public Deployment Deployment { get; set; }

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
            Deployment = await _appContext.Deployments
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (Deployment == null)
            {
                return NotFound();
            }

            ClientId = Deployment.ClientId;
            Clients = await _identityContext.Clients
                //.Where(client => user.ClientIds.Contains(client.Id))
                .OrderBy(client => client.ClientId)
                .Select(client => new SelectListItem
                {
                    Text = client.ClientName,
                    Value = client.Id.ToString()
                })
                .ToListAsync();

            ToolPlacements = Enum.GetNames(typeof(Deployment.ToolPlacements))
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

            Deployment.ClientId = ClientId;

            _appContext.Attach(Deployment).State = EntityState.Modified;

            try
            {
                await _appContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeploymentExists(Deployment.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool DeploymentExists(int id)
        {
            return _appContext.Deployments.Any(e => e.Id == id);
        }
    }
}
