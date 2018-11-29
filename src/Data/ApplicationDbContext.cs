using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<AdvantagePlatformUser>
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<ResourceLink> ResourceLinks { get; set; }
        public DbSet<Tool> Tools { get; set; }

        /// <summary>
        /// Returns the fully populated <see cref="AdvantagePlatformUser"/> corresponding to the
        /// IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal or null.
        /// </summary>
        /// <param name="principal">The principal which contains the user id claim.</param>
        /// <returns>The user corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType claim in
        /// the principal or null</returns>
        public async Task<AdvantagePlatformUser> GetUserAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var id = GetUserId(principal);
            return await GetUserAsync(id);
        }

        /// <summary>
        /// Returns the fully populated <see cref="AdvantagePlatformUser"/> corresponding to the
        /// IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal or null.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <returns>The user corresponding to the user id.</returns>
        public async Task<AdvantagePlatformUser> GetUserAsync(string id)
        {
            if (id == null)
            {
                return null;
            }

            return await Users
                .Include(u => u.Course)
                .ThenInclude(c => c.ResourceLinks)
                .Include(u => u.People)
                .Include(u => u.Platform)
                .ThenInclude(p => p.ResourceLinks)
                .Include(u => u.ResourceLinks)
                .ThenInclude(r => r.Tool)
                .Include(u => u.Tools)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        private string GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal.FindFirstValue("sub");
        }
    }
}
