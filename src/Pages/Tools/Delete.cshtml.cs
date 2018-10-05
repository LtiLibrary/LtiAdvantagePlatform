using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;

namespace AdvantagePlatform.Pages.Tools
{
    public class DeleteModel : PageModel
    {
        private readonly AdvantagePlatform.Data.ApplicationDbContext _context;

        public DeleteModel(AdvantagePlatform.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Tool Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Tool = await _context.Tools.FirstOrDefaultAsync(m => m.Id == id);

            if (Tool == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Tool = await _context.Tools.FindAsync(id);

            if (Tool != null)
            {
                _context.Tools.Remove(Tool);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
