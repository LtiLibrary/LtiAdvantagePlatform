using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Areas.Identity.Pages.Account.Manage
{
    public class PeopleModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public PeopleModel(ApplicationDbContext context, UserManager<AdvantagePlatformUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Person Student { get; set; }

        [BindProperty]
        public Person Teacher { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Student = await _context.People.SingleOrDefaultAsync(
                c => c.UserId == user.Id && c.IsStudent);
            if (Student == null)
            {
                Student = RegisterModel.CreatePerson(user, true);
                await _context.People.AddAsync(Student);
                await _context.SaveChangesAsync();
                user.StudentId = Student.Id;
                await _userManager.UpdateAsync(user);
            }

            Teacher = await _context.People.SingleOrDefaultAsync(
                c => c.UserId == user.Id && !c.IsStudent);
            if (Teacher == null)
            {
                Teacher = RegisterModel.CreatePerson(user, false);
                await _context.People.AddAsync(Teacher);
                await _context.SaveChangesAsync();
                user.TeacherId = Teacher.Id;
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

            _context.Attach(Student).State = EntityState.Modified;
            _context.Attach(Teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Page();
        }
    }
}