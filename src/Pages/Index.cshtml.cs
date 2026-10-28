using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc;
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
        public IList<CourseGroupSet> GroupSets { get; set; } = new List<CourseGroupSet>();

        public async Task OnGetAsync()
        {
            var user = await _context.GetUserLightAsync(User);
            if (user != null)
            {
                Platform = user.Platform;
                Course = user.Course;
                People = user.People.ToList();
                GroupSets = await _context.GetGroupSetsAsync(Course.Id);
            }
        }

        /// <summary>
        /// Removes a deep-linked content item (link, HTML, file or image) from the platform or course.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteContentAsync(int id)
        {
            var user = await _context.GetUserFullAsync(User);
            var content = user?.Course.LinkedContent.Concat(user.Platform.LinkedContent)
                .SingleOrDefault(c => c.Id == id);
            if (content != null)
            {
                _context.LinkedContent.Remove(content);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        /// <summary>
        /// The instructor releases or hides a gradebook column's grades. The tool sees the new
        /// gradesReleased the next time it reads the line item.
        /// </summary>
        public async Task<IActionResult> OnPostSetGradesReleasedAsync(int id, bool? released)
        {
            if (released == null)
            {
                return BadRequest();
            }

            var user = await _context.GetUserLightAsync(User);
            var course = user?.Course == null ? null : await _context.GetCourseAsync(user.Course.Id);
            var column = course?.GradebookColumns.SingleOrDefault(c => c.Id == id);
            if (column != null)
            {
                column.GradesReleased = released;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
