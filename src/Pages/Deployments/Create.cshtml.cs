using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Deployments
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Deployment Deployment { get; set; }

        [BindProperty]
        [Required]
        [Display(Name = "Tool")]
        public int ToolId { get; set; }
        public IList<SelectListItem> Tools { get; private set; }

        [BindProperty]
        [Required]
        [Display(Name = "Client")]
        public string ClientId { get; set; }
        public IList<SelectListItem> Clients { get; private set; }

        public IList<SelectListItem> ToolPlacements { get; private set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            
            Tools = await _context.Tools
                .Where(t => t.UserId == user.Id)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            Clients = await _context.Clients
                .Where(c => c.UserId == user.Id)
                .Select(c => new SelectListItem
                {
                    Value = c.Id,
                    Text = c.Name
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
            Deployment.Tool = await _context.Tools.FindAsync(ToolId);
            Deployment.Client = await _context.Clients.FindAsync(ClientId);

            _context.Deployments.Add(Deployment);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}