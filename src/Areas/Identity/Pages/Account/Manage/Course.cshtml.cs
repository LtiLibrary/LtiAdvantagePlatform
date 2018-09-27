using System.Threading.Tasks;
using AdvantagePlatform.Data;
using LtiAdvantageLibrary.NetCore.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Areas.Identity.Pages.Account.Manage
{
    public class CourseModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public CourseModel(
            ApplicationDbContext context,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Course Course { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Course = await _context.Courses.SingleOrDefaultAsync(c => c.UserId == user.Id);
            if (Course == null)
            {
                Course = new Course { UserId = user.Id };
                await _context.Courses.AddAsync(Course);
                await _context.SaveChangesAsync();
                user.CourseId = Course.Id;
                await _userManager.UpdateAsync(user);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Course).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}