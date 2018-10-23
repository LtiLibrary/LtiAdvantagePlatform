using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AdvantagePlatform.Pages.Deployments
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

        public IList<DeploymentModel> Deployments { get;set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            Deployments = await GetDeplomentModelsAsync(user.Id);
        }

        private async Task<IList<DeploymentModel>> GetDeplomentModelsAsync(string userId)
        {
            var list = new List<DeploymentModel>();

            var deployments = _appContext.Deployments
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.ClientId);

            Client client = null;
            foreach (var deployment in deployments)
            {
                if (client == null || client.Id != deployment.ClientId)
                {
                    client = await _identityContext.Clients.FindAsync(deployment.ClientId);
                }

                list.Add(new DeploymentModel
                {
                    Id = deployment.Id,
                    ToolName = deployment.ToolName,
                    ToolPlacement = deployment.ToolPlacement,
                    ToolUrl = deployment.ToolUrl,
                    ClientName = client == null ? "[No Client]" : client.ClientName
                });
            }

            list = list.OrderBy(d => d.ToolName).ToList();

            return list;
        }
    }
}
