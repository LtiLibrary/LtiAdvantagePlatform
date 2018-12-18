﻿using System.Collections.Generic;
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
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityConfig;

        public IndexModel(
            ApplicationDbContext context,
            IConfigurationDbContext identityConfig)
        {
            _context = context;
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
            var user = await _context.GetUserAsync(User);
            if (user != null)
            {
                Platform = user.Platform;
                Course = user.Course;
                People = user.People.ToList();
                Tools = await GetTools(user);
                CourseResourceLinks = ResourceLinkModel.GetResourceLinks(user.Course.ResourceLinks);
                PlatformResourceLinks = ResourceLinkModel.GetResourceLinks(user.Platform.ResourceLinks);
            }
        }

        private async Task<IList<ToolModel>> GetTools(AdvantagePlatformUser user)
        {
            var list = new List<ToolModel>();
            foreach (var tool in user.Tools.OrderBy(t => t.Name).ToList())
            {
                var client = await _identityConfig.Clients.FindAsync(tool.IdentityServerClientId);

                list.Add(new ToolModel(Request.HttpContext, tool, client));
            }

            return list;
        }
    }
}
