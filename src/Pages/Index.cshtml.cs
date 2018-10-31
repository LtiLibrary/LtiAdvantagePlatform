using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using AdvantagePlatform.Pages.Tools;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityConfig;
        private readonly UserManager<AdvantagePlatformUser> _userManager;

        public IndexModel(
            ApplicationDbContext appContext,
            IConfigurationDbContext identityConfig,
            UserManager<AdvantagePlatformUser> userManager)
        {
            _appContext = appContext;
            _identityConfig = identityConfig;
            _userManager = userManager;
        }

        public Platform Platform { get; set; }
        public Course Course { get; set; }
        public Person Teacher { get; set; }
        public Person Student { get; set; }
        public IList<ToolModel> Tools { get; set; }
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

                Tools = await GetTools(user.Id);

                PlatformResourceLinks = await GetResourceLinksAsync(ResourceLink.LinkContexts.Platform, user.Id);
                CourseResourceLinks = await GetResourceLinksAsync(ResourceLink.LinkContexts.Course, user.Id);
            }
        }

        private async Task<IList<ResourceLinkModel>> GetResourceLinksAsync(ResourceLink.LinkContexts context,
            string userId)
        {
            var list = new List<ResourceLinkModel>();

            var resourceLinks = _appContext.ResourceLinks
                .Where(d => d.LinkContext == context
                            && d.UserId == userId)
                .OrderBy(d => d.Title);

            foreach (var link in resourceLinks)
            {
                var tool = await _appContext.Tools.FindAsync(link.ToolId);

                list.Add(new ResourceLinkModel
                {
                    Id = link.Id,
                    Title = link.Title,
                    ToolName = tool.ToolName,
                    LinkContext = link.LinkContext
                });
            }

            return list;
        }

        private async Task<IList<ToolModel>> GetTools(string userId)
        {
            var tools = _appContext.Tools
                .Where(tool => tool.UserId == userId)
                .OrderBy(tool => tool.ToolName);

            var list = new List<ToolModel>();
            foreach (var tool in tools)
            {
                var client = await _identityConfig.Clients.FindAsync(tool.IdentSvrClientId);

                list.Add(new ToolModel
                {
                    ToolClientId = client.ClientId,
                    DeploymentId = tool.DeploymentId,
                    ToolName = tool.ToolName,
                    ToolUrl = tool.ToolUrl
                });
            }

            return list;
        }
    }
}
