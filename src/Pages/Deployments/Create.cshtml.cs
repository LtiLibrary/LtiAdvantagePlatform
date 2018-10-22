using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Deployments
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
        public Deployment Deployment { get; set; }

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
                .Where(client => user.ClientIds.Contains(client.Id))
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

            var user = await _userManager.GetUserAsync(User);
            Deployment.UserId = user.Id;
            Deployment.ClientId = ClientId;

            _appContext.Deployments.Add(Deployment);
            await _appContext.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}