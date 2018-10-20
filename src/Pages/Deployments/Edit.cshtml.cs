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
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Deployments
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
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

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            Deployment = await _context.Deployments
                .Include(m => m.Client)
                .Include(m => m.Tool)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (Deployment == null)
            {
                return NotFound();
            }

            ToolId = Deployment.Tool.Id;
            Tools = await _context.Tools
                .Where(t => t.UserId == user.Id)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                })
                .ToListAsync();

            ClientId = Deployment.Client.Id;
            Clients = await _context.MyClients
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

            Deployment.Client = await _context.MyClients.FindAsync(ClientId);
            Deployment.Tool = await _context.Tools.FindAsync(ToolId);

            _context.Attach(Deployment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
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

        private bool DeploymentExists(string id)
        {
            return _context.Deployments.Any(e => e.Id == id);
        }
    }
}
