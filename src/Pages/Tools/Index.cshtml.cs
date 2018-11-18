using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;

        public IndexModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        public IList<ToolModel> Tools { get;set; }

        public async Task OnGetAsync()
        {
            Tools = await GetToolListAsync();
        }

        private async Task<IList<ToolModel>> GetToolListAsync()
        {
            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            var list = new List<ToolModel>();
            foreach (var tool in user.Tools.OrderBy(t => t.Name))
            {
                var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);
                var item = new ToolModel(tool, client);
                list.Add(item);
            }

            return list;
        }
    }
}
