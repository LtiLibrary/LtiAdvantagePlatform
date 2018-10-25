using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
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
        public IList<ResourceLinkModel> PlatformResourceLinks { get; set; }
        public IList<ResourceLinkModel> CourseResourceLinks { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Platform = await _appContext.Platforms.FindAsync(user.PlatformId);
                Course = await _appContext.Courses.FindAsync(user.CourseId);
                Teacher = await _appContext.People.FindAsync(user.TeacherId);
                Student = await _appContext.People.FindAsync(user.StudentId);

                PlatformResourceLinks = await GetDeplomentModelsAsync(ResourceLink.ToolPlacements.Platform, user.Id);
                CourseResourceLinks = await GetDeplomentModelsAsync(ResourceLink.ToolPlacements.Course, user.Id);
            }
        }

        private async Task<IList<ResourceLinkModel>> GetDeplomentModelsAsync(ResourceLink.ToolPlacements placement,
            string userId)
        {
            var list = new List<ResourceLinkModel>();

            var resourceLinks = _appContext.ResourceLinks
                .Where(d => d.ToolPlacement == placement
                            && d.UserId == userId)
                .OrderBy(d => d.ClientId);

            Client client = null;
            foreach (var resourceLink in resourceLinks)
            {
                if (client == null || client.Id != resourceLink.ClientId)
                {
                    client = await _identityContext.Clients.FindAsync(resourceLink.ClientId);
                }

                list.Add(new ResourceLinkModel
                {
                    Id = resourceLink.Id,
                    ClientName = client == null ? "[No Client]" : client.ClientName,
                    DeploymentId = resourceLink.DeploymentId,
                    ToolName = resourceLink.ToolName,
                    ToolUrl = resourceLink.ToolUrl
                });
            }

            list = list.OrderBy(d => d.ToolName).ToList();

            return list;
        }
    }
}
