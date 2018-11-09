using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityContext;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(
            ApplicationDbContext appContext,
            IConfigurationDbContext identityContext, 
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityContext = identityContext;
            _userManager = userManager;
        }

        public IList<ToolModel> Tools { get;set; }

        public async Task OnGetAsync()
        {
            Tools = await BuildClientListAsync();
        }

        private async Task<IList<ToolModel>> BuildClientListAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var tools = _appContext.Tools
                .Where(tool => tool.UserId == user.Id)
                .OrderBy(tool => tool.Name);

            var list = new List<ToolModel>();
            foreach (var tool in tools)
            {
                var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);

                var item = new ToolModel
                {
                    Id = tool.Id,
                    ClientId = client.ClientId,
                    DeploymentId = tool.DeploymentId,
                    Name = tool.Name,
                    Url = tool.Url
                };

                list.Add(item);
            }

            return list;
        }
    }
}
