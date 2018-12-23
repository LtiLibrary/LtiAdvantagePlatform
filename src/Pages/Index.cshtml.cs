using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Platform Platform { get; set; }
        public Course Course { get; set; }
        public IList<Person> People { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _context.GetUserLightAsync(User);
            if (user != null)
            {
                Platform = user.Platform;
                Course = user.Course;
                People = user.People.ToList();
            }
        }
    }
}
