using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class LaunchPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LaunchPageModel> _logger;

        public LaunchPageModel(ApplicationDbContext context, ILogger<LaunchPageModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Verify that everything is ready for launch.
        /// </summary>
        /// <param name="id">The <see cref="ResourceLink"/> id.</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _logger.LogError(new ArgumentNullException(nameof(id)), "Missing resource link id.");
                return NotFound();
            }
            
            var user = await _context.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("User not found.");
                return NotFound();
            }

            var resourceLink = user.ResourceLinks.SingleOrDefault(r => r.Id == id);
            if (resourceLink == null)
            {
                _logger.LogError("Resource link not found.");
                return NotFound();
            }

            var tool = resourceLink.Tool;
            if (tool == null)
            {
                _logger.LogError("Tool not found.");
                return NotFound();
            }

            _logger.LogInformation($"Launching {resourceLink.Title}.");
            ViewData["Title"] = resourceLink.Title;
            return Page();
        }
    }
}