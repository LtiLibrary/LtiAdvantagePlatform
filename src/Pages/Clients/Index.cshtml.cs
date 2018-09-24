using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Client> Client { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Client = await _context.Clients.Where(c => c.CreatorId == user.Id).ToListAsync();
        }
    }
}
