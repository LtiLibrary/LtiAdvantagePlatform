using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Pages.Components.DeepLinks
{
    public class DeepLinksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public DeepLinksViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? courseId = null)
        {
            var model = new DeepLinksViewComponentModel { CourseId = courseId };

            var user = await _context.GetUserLightAsync(HttpContext.User);
            if (user != null)
            {
                model.People = user.People.ToList();
                model.Tools = user.Tools
                    .Where(t => t.DeepLinkingLaunchUrl.IsPresent())
                    .OrderBy(t => t.Name)
                    .Select(t => new ToolModel(HttpContext, t))
                    .ToList();
            }

            return View(model);
        }
    }
}
