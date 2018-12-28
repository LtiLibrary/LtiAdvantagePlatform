using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using Microsoft.AspNetCore.Mvc;

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
            return View(model);
        }
    }
}
