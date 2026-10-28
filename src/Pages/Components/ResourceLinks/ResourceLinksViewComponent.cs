using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Components.ResourceLinks
{
    public class ResourceLinksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ResourceLinksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? courseId = null)
        {
            var model = new ResourceLinksViewComponentModel {CourseId = courseId};

            var user = await _context.GetUserFullAsync(HttpContext.User);
            model.People = user.People.ToList();
            model.ResourceLinks = ResourceLinkModel.GetResourceLinks(courseId.HasValue 
                ? user.Course.ResourceLinks 
                : user.Platform.ResourceLinks);
            model.LinkedContent = (courseId.HasValue
                    ? user.Course.LinkedContent
                    : user.Platform.LinkedContent)
                .OrderBy(c => c.Title)
                .ToList();

            var proctoredIds = model.ResourceLinks.Where(l => l.IsProctored).Select(l => l.Id).ToList();
            model.ProctoringAttempts = await _context.ProctoringAttempts
                .Where(a => proctoredIds.Contains(a.ResourceLinkId))
                .Include(a => a.Events)
                .OrderByDescending(a => a.Id)
                .ToListAsync();
            return View(model);
        }
    }
}
