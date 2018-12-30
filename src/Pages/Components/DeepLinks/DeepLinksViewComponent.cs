using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Pages.Components.DeepLinks
{
    public class DeepLinksViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityConfig;

        public DeepLinksViewComponent(
            ApplicationDbContext context,
            IConfigurationDbContext identityConfig)
        {
            _context = context;
            _identityConfig = identityConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? courseId = null)
        {
            var model = new DeepLinksViewComponentModel {CourseId = courseId};

            var user = await _context.GetUserLightAsync(HttpContext.User);
            model.People = user.People.ToList();
            model.Tools = await GetTools(user);
            return View(model);
        }
        
        private async Task<IList<ToolModel>> GetTools(AdvantagePlatformUser user)
        {
            var tools = user.Tools
                .Where(t => t.DeepLinkingLaunchUrl.IsPresent())
                .OrderBy(t => t.Name)
                .ToList();

            var toolModels = new List<ToolModel>();

            foreach (var tool in tools)
            {
                var client = await _identityConfig.Clients.FindAsync(tool.IdentityServerClientId);

                toolModels.Add(new ToolModel(HttpContext, tool, client));
            }

            return toolModels;
        }
    }
}
