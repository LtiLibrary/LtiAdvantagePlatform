using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;

namespace AdvantagePlatform.Utility
{
    public class CourseAccessValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<CourseAccessValidator> _logger;

        public CourseAccessValidator(
            ApplicationDbContext context,
            IHttpContextAccessor httpContext,
            ILogger<CourseAccessValidator> logger)
        {
            _context = context;
            _httpContext = httpContext;
            _logger = logger;
        }

        public async Task<bool> UserHasAccess(int courseId)
        {
            _logger.LogInformation("Validating access to course {CourseId}.", courseId);
            var httpContext = _httpContext.HttpContext;

            if (httpContext?.User == null || !httpContext.User.Identity!.IsAuthenticated)
            {
                _logger.LogError("Request is not authenticated.");
                return false;
            }

            // Application user (browser session) — owns the course directly.
            var userId = _context.GetUserId(httpContext.User);
            if (userId != null)
            {
                if (await _context.Users.AnyAsync(u => u.Id == userId && u.Course.Id == courseId))
                {
                    return true;
                }
            }

            // API client (tool) — its ClientId claim points to the Tool that belongs
            // to one of the application users. Allow access if that tool's owner
            // owns the requested course.
            var clientId = GetClientId(httpContext.User);
            if (clientId != null)
            {
                return await _context.Users
                    .AnyAsync(u => u.Course.Id == courseId
                                   && u.Tools.Any(t => t.ClientId == clientId));
            }

            _logger.LogError("ClaimsPrincipal not recognized.");
            return false;
        }

        private static string GetClientId(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(OpenIddictConstants.Claims.ClientId)
                   ?? principal.FindFirstValue("client_id");
        }
    }
}
