using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Deployments;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages
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

        public Platform Platform { get; set; }
        public Course Course { get; set; }
        public Person Teacher { get; set; }
        public Person Student { get; set; }
        public IList<DeploymentModel> PlatformTools { get; set; }
        public IList<DeploymentModel> CourseTools { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Platform = await _appContext.Platforms.FindAsync(user.PlatformId);
                Course = await _appContext.Courses.FindAsync(user.CourseId);
                Teacher = await _appContext.People.FindAsync(user.TeacherId);
                Student = await _appContext.People.FindAsync(user.StudentId);

                PlatformTools = await GetDeplomentModelsAsync(Deployment.ToolPlacements.Platform, user.Id);
                CourseTools = await GetDeplomentModelsAsync(Deployment.ToolPlacements.Course, user.Id);
            }
        }

        private async Task<IList<DeploymentModel>> GetDeplomentModelsAsync(Deployment.ToolPlacements placement,
            string userId)
        {
            var list = new List<DeploymentModel>();

            var deployments = _appContext.Deployments
                .Where(d => d.ToolPlacement == placement
                            && d.UserId == userId)
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
                    ToolUrl = deployment.ToolUrl,
                    ClientName = client == null ? "[No Client]" : client.ClientName
                });
            }

            list = list.OrderBy(d => d.ToolName).ToList();

            return list;
        }
    }
}
