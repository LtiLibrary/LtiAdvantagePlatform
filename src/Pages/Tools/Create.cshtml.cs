using System;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvantagePlatform.Pages.Tools
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolClientManager _toolClientManager;

        [BindProperty]
        public ToolModel Tool { get; set; }

        public CreateModel(ApplicationDbContext context, ToolClientManager toolClientManager)
        {
            _context = context;
            _toolClientManager = toolClientManager;
        }

        public IActionResult OnGet()
        {
            Tool = new ToolModel(Request.HttpContext)
            {
                ClientId = Guid.NewGuid().ToString("N").Substring(0, 16),
                DeploymentId = Guid.NewGuid().ToString("N").Substring(0, 16)
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Tool.CustomProperties.IsPresent() && !Tool.CustomProperties.TryConvertToDictionary(out _))
            {
                ModelState.AddModelError(
                    $"{nameof(Tool)}.{nameof(Tool.CustomProperties)}",
                    "Cannot parse the Custom Properties.");
            }

            if (await _toolClientManager.ExistsAsync(Tool.ClientId))
            {
                ModelState.AddModelError($"{nameof(Tool)}.{nameof(Tool.ClientId)}",
                    "This Client ID already exists.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _toolClientManager.CreateAsync(Tool.ClientId, Tool.Name, Tool.LaunchUrl);

            var tool = new Tool
            {
                ClientId = Tool.ClientId,
                CustomProperties = Tool.CustomProperties,
                DeepLinkingLaunchUrl = Tool.DeepLinkingLaunchUrl,
                DeploymentId = Tool.DeploymentId,
                LaunchUrl = Tool.LaunchUrl,
                LoginUrl = Tool.LoginUrl,
                Name = Tool.Name,
                PublicKey = Tool.PublicKey
            };
            await _context.Tools.AddAsync(tool);

            var user = await _context.GetUserLightAsync(User);
            user.Tools.Add(tool);

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
