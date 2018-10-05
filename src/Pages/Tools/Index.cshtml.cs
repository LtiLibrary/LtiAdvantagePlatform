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
    public class IndexModel : PageModel
    {
        private readonly AdvantagePlatform.Data.ApplicationDbContext _context;

        public IndexModel(AdvantagePlatform.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Tool> Tool { get;set; }

        public async Task OnGetAsync()
        {
            Tool = await _context.Tools.ToListAsync();
        }
    }
}
