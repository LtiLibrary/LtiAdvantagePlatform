using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
        public IList<Deployment> PlatformTools { get; set; }
        public IList<Deployment> CourseTools { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Platform = await _context.Platforms.FindAsync(user.PlatformId);
                Course = await _context.Courses.FindAsync(user.CourseId);
                Teacher = await _context.People.FindAsync(user.TeacherId);
                Student = await _context.People.FindAsync(user.StudentId);

                PlatformTools = await _context.Deployments
                    .Include(d => d.Client)
                    .Include(d => d.Tool)
                    .Where
                    (
                        d => d.ToolPlacement == Deployment.ToolPlacements.Platform
                             && d.UserId == user.Id
                    )
                    .ToListAsync();

                CourseTools = await _context.Deployments
                    .Include(d => d.Client)
                    .Include(d => d.Tool)
                    .Where
                    (
                        d => d.ToolPlacement == Deployment.ToolPlacements.Course
                             && d.UserId == user.Id
                    )
                    .ToListAsync();
            }
        }
    }
}
