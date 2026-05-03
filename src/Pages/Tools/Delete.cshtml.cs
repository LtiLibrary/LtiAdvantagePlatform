using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolClientManager _toolClientManager;

        public DeleteModel(ApplicationDbContext context, ToolClientManager toolClientManager)
        {
            _context = context;
            _toolClientManager = toolClientManager;
        }

        public ToolModel Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.GetUserLightAsync(User);
            if (user == null) return NotFound();

            var tool = user.Tools.SingleOrDefault(t => t.Id == id);
            if (tool == null) return NotFound();

            Tool = new ToolModel(Request.HttpContext, tool);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return Page();

            var tool = await _context.Tools.FindAsync(id);
            if (tool != null)
            {
                await _toolClientManager.DeleteAsync(tool.ClientId);

                _context.Tools.Remove(tool);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
