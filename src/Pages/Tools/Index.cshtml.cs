using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ToolModel> Tools { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _context.GetUserLightAsync(User);
            if (user == null)
            {
                Tools = new List<ToolModel>();
                return;
            }

            Tools = user.Tools
                .OrderBy(t => t.Name)
                .Select(t => new ToolModel(Request.HttpContext, t))
                .ToList();
        }
    }
}
