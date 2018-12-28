using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Pages.Tools;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AdvantagePlatform.Pages.Components.Tools
{
    public class ToolsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityConfig;

        public ToolsViewComponent(
            ApplicationDbContext context,
            IConfigurationDbContext identityConfig)
        {
            _context = context;
            _identityConfig = identityConfig;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new ToolsViewComponentModel();
            var user = await _context.GetUserLightAsync(HttpContext.User);
            model.Tools = await GetTools(user);
            return View(model);
        }
        
        private async Task<IList<ToolModel>> GetTools(AdvantagePlatformUser user)
        {
            var list = new List<ToolModel>();

            foreach (var tool in user.Tools.OrderBy(t => t.Name).ToList())
            {
                var client = await _identityConfig.Clients.FindAsync(tool.IdentityServerClientId);

                list.Add(new ToolModel(HttpContext, tool, client));
            }

            return list;
        }
    }
}
