using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Data.Migrations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Platform = AdvantagePlatform.Data.Platform;

namespace AdvantagePlatform.Pages
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

        public Platform Platform { get; set; }
        public Course Course { get; set; }
        public Person Teacher { get; set; }
        public Person Student { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            Platform = await _context.Platforms.FindAsync(user.PlatformId);
            Course = await _context.Courses.FindAsync(user.CourseId);
            Teacher = await _context.People.FindAsync(user.TeacherId);
            Student = await _context.People.FindAsync(user.StudentId);
        }
    }
}
