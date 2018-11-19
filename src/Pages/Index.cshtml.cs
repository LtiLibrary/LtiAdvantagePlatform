using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.ResourceLinks;
using AdvantagePlatform.Pages.Tools;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _appContext;
        private readonly IConfigurationDbContext _identityConfig;

        public IndexModel(
            ApplicationDbContext appContext,
            IConfigurationDbContext identityConfig)
        {
            _appContext = appContext;
            _identityConfig = identityConfig;
        }

        public Platform Platform { get; set; }
        public Course Course { get; set; }
        public IList<Person> People { get; set; }
        public IList<ToolModel> Tools { get; set; }
        public IList<ResourceLinkModel> PlatformResourceLinks { get; set; }
        public IList<ResourceLinkModel> CourseResourceLinks { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _appContext.GetUserAsync(User);
            if (user != null)
            {
                Platform = user.Platform;
                Course = user.Course;
                People = user.People.ToList();
                Tools = await GetTools(user);
                PlatformResourceLinks = GetResourceLinks(ResourceLink.LinkContexts.Platform, user);
                CourseResourceLinks = GetResourceLinks(ResourceLink.LinkContexts.Course, user);
            }
        }

        private IList<ResourceLinkModel> GetResourceLinks(ResourceLink.LinkContexts context, AdvantagePlatformUser user)
        {
            var list = new List<ResourceLinkModel>();

            var links = user.ResourceLinks
                .Where(r => r.LinkContext == context)
                .OrderBy(r => r.Title)
                .ToList();

            foreach (var link in links)
            {
                if (link.Tool != null)
                {
                    list.Add(new ResourceLinkModel
                    {
                        Id = link.Id,
                        Title = link.Title,
                        ToolName = link.Tool.Name,
                        LinkContext = link.LinkContext
                    });
                }
            }

            return list;
        }

        private async Task<IList<ToolModel>> GetTools(AdvantagePlatformUser user)
        {
            var list = new List<ToolModel>();
            foreach (var tool in user.Tools.OrderBy(t => t.Name).ToList())
            {
                var client = await _identityConfig.Clients.FindAsync(tool.IdentityServerClientId);

                list.Add(new ToolModel
                {
                    ClientId = client.ClientId,
                    DeploymentId = tool.DeploymentId,
                    Name = tool.Name,
                    LaunchUrl = tool.LaunchUrl
                });
            }

            return list;
        }
    }
}
