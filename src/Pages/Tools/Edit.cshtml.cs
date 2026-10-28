using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Pages.Models;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Tools
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ToolClientManager _toolClientManager;
        private readonly ToolSigningKeys _toolSigningKeys;

        public EditModel(ApplicationDbContext context, ToolClientManager toolClientManager, ToolSigningKeys toolSigningKeys)
        {
            _context = context;
            _toolClientManager = toolClientManager;
            _toolSigningKeys = toolSigningKeys;
        }

        [BindProperty]
        public ToolModel Tool { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.GetUserLightAsync(User);
            if (user == null) return NotFound();

            var tool = user.Tools.SingleOrDefault(t => t.Id == id);
            if (tool == null) return NotFound();

            Tool = new ToolModel(Request.HttpContext, tool);
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

            var keySet = await Tool.ValidateKeysAsync(ModelState, _toolSigningKeys);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var tool = await _context.Tools.FindAsync(Tool.Id);
            tool.CustomProperties = Tool.CustomProperties;
            tool.DeepLinkingLaunchUrl = Tool.DeepLinkingLaunchUrl;
            tool.LaunchUrl = Tool.LaunchUrl;
            tool.LoginUrl = Tool.LoginUrl;
            tool.Name = Tool.Name;
            tool.JwksUrl = Tool.JwksUrl;
            tool.PublicKey = Tool.PublicKey;
            tool.RedirectUris = Tool.RedirectUris;

            _context.Tools.Attach(tool).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _toolClientManager.UpdateAsync(tool.ClientId, tool.Name, tool.GetAllRedirectUris(), keySet);

            return RedirectToPage("./Index");
        }
    }
}
