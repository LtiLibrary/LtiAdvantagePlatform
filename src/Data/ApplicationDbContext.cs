using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using LtiAdvantageLibrary;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AdvantagePlatformUser>()
                .HasOne(u => u.Platform)
                .WithOne(p => p.User)
                .HasForeignKey<Platform>(p => p.UserId);

            builder.Entity<AdvantagePlatformUser>()
                .HasOne(u => u.Course)
                .WithOne(p => p.User)
                .HasForeignKey<Course>(p => p.UserId);

            base.OnModelCreating(builder);
        }

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
            if (id == null)
            {
                return null;
            }

            return await Users
                .Include(u => u.Course)
                .Include(u => u.People)
                .Include(u => u.Platform)
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
