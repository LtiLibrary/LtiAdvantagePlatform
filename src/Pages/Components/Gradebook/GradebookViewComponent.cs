using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Pages.Components.Gradebook
{
    public class GradebookViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public GradebookViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int courseId)
        {
            var model = new GradebookViewComponentModel();

            var user = await _context.GetUserAsync(HttpContext.User);
            model.Members = new Dictionary<int, string>();
            foreach (var person in user.People.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
            {
                if (!model.Members.ContainsKey(person.Id))
                {
                    model.Members.Add(person.Id, $"{person.LastName}, {person.FirstName}");
                }
            }

            var course = await _context.GetCourseAsync(courseId);
            model.Columns = course.GradebookColumns
                .Select(c => new GradebookViewComponentModel.MyGradebookColumn
                {
                    Column = c,
                    Header = c.Label ?? $"Tag: {c.Tag}"
                })
                .ToList();

            return View(model);
        }
    }
}
