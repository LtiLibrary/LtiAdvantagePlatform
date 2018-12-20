using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Utility
{
    public class CourseAccessValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityConfig;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILogger<CourseAccessValidator> _logger;

        public CourseAccessValidator(
            ApplicationDbContext context,
            IConfigurationDbContext identityConfig,
            IHttpContextAccessor httpContext,
            ILogger<CourseAccessValidator> logger)
        {
            _context = context;
            _identityConfig = identityConfig;
            _httpContext = httpContext;
            _logger = logger;
        }

        public async Task<bool> UserHasAccess(int courseId)
        {
            _logger.LogInformation($"Validating access to course [{courseId}].");
            var httpContext = _httpContext.HttpContext;

            // This must be an authenticated request
            if (httpContext?.User == null || !httpContext.User.Identity.IsAuthenticated)
            {
                _logger.LogError("Request is not authenticated.");
                return false;
            }

            // First check if this is an application user
            var userId = _context.GetUserId(httpContext.User);
            if (userId != null)
            {
                return await _context.Users
                    .Include(u => u.Course)
                    .AnyAsync(u => u.Course.Id == courseId);
            }

            // If not an application user, then maybe an API client
            var client =
                await _identityConfig.Clients
                    .SingleOrDefaultAsync(c => c.ClientId == GetClientId(httpContext.User));
            if (client != null)
            {
                return await _context.Users
                    .Include(u => u.Tools)
                    .AnyAsync(u => u.Tools.Any(t => t.IdentityServerClientId == client.Id));
            }

            _logger.LogError("ClaimsPrincipal not recognized.");
            return false;
        }

        private static string GetClientId(ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(IdentityModel.JwtClaimTypes.ClientId);
        }
    }
}
