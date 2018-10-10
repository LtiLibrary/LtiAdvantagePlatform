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
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Client> Clients { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Clients = await _context.Clients
                .Where(c => c.UserId == user.Id)
                .ToListAsync();
        }
    }
}
