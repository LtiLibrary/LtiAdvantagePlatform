using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Pages.Components.Tools
{
    public class ToolsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ToolsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ToolsViewComponentModel();
            var user = await _context.GetUserLightAsync(HttpContext.User);
            model.Tools = GetTools(user);
            return View(model);
        }

        private IList<ToolModel> GetTools(AdvantagePlatformUser user)
        {
            if (user == null) return new List<ToolModel>();

            return user.Tools
                .OrderBy(t => t.Name)
                .Select(t => new ToolModel(HttpContext, t))
                .ToList();
        }
    }
}
