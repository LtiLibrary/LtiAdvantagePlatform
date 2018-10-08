using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Deployments
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Deployment Deployment { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Deployment = await _context.Deployments.FirstOrDefaultAsync(m => m.Id == id);

            if (Deployment == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
