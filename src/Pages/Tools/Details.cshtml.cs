using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Tools
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;

        public DetailsModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public ToolModel Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var tool = user.Tools.SingleOrDefault(t => t.Id == id);
            if (tool == null)
            {
                return NotFound();
            }

            var client = await _identityContext.Clients
                .Include(c => c.ClientSecrets)
                .SingleOrDefaultAsync(c => c.Id == tool.IdentityServerClientId);
            if (client == null)
            {
                return NotFound();
            }

            Tool = new ToolModel(tool, client);

            return Page();
        }
    }
}
