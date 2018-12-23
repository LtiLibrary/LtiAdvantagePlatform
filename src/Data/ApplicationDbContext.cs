using System;
using System.Linq;
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
        public DbSet<GradebookColumn> GradebookColumns { get; set; }
        public DbSet<GradebookRow> GradebookRows { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<ResourceLink> ResourceLinks { get; set; }
        public DbSet<Tool> Tools { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GradebookColumn>().HasMany(c => c.Scores).WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ResourceLink>().HasMany<GradebookColumn>().WithOne(g => g.ResourceLink)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Tool>().HasMany<ResourceLink>().WithOne(l => l.Tool)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }

        #region Convenience methods

        /// <summary>
        /// Return a fully populated <see cref="Course"/>.
        /// </summary>
        /// <param name="id">The course id.</param>
        /// <returns>The course.</returns>
        public async Task<Course> GetCourseAsync(int id)
        {
            return await Courses
                .Include(c => c.GradebookColumns)
                .ThenInclude(c => c.ResourceLink)
                .Include(c => c.GradebookColumns)
                .ThenInclude(c => c.Scores)
                .Include(c => c.ResourceLinks)
                .ThenInclude(l => l.Tool)
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Return a course given the id of a resource link within the course.
        /// </summary>
        /// <param name="resourceLinkId">The resource link id.</param>
        /// <returns>The course.</returns>
        public async Task<Course> GetCourseByResourceLinkIdAsync(int resourceLinkId)
        {
            var course = await Courses.SingleOrDefaultAsync(c => c.ResourceLinks.Any(l => l.Id == resourceLinkId));

            return course == null ? null  : await GetCourseAsync(course.Id);
        }
                
        /// <summary>
        /// Returns a gradebook column with scores.
        /// </summary>
        /// <param name="id">The gradebook column id.</param>
        /// <returns>The gradebook column.</returns>
        public async Task<GradebookColumn> GetGradebookColumnAsync(int id)
        {
            return await GradebookColumns
                .Include(c => c.Scores)
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Returns the gradebook column for a resource link if there is exactly one. Otherwise
        /// returns null.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The gradebook column.</returns>
        public async Task<GradebookColumn> GetGradebookColumnByResourceLinkIdAsync(int id)
        {
            var gradebooksColumns = await GradebookColumns.Where(c => c.ResourceLink.Id == id).ToListAsync();
            return gradebooksColumns.Count == 1 ? gradebooksColumns[0] : null;
        }
                
        /// <summary>
        /// Returns a gradebook row.
        /// </summary>
        /// <param name="id">The gradebook row id.</param>
        /// <returns>The gradebook row.</returns>
        public async Task<GradebookRow> GetGradebookRowAsync(int id)
        {
            return await GradebookRows.FindAsync(id);
        }

        /// <summary>
        /// Returns a person.
        /// </summary>
        /// <param name="id">The person id.</param>
        /// <returns>The person.</returns>
        public async Task<Person> GetPersonAsync(int id)
        {
            return await People.FindAsync(id);
        }

        /// <summary>
        /// Returns a full platform.
        /// </summary>
        /// <param name="id">The platform id.</param>
        /// <returns>The platform.</returns>
        public async Task<Platform> GetPlatformAsync(int id)
        {
            return await Platforms
                .Include(p => p.ResourceLinks)
                .SingleOrDefaultAsync(p => p.Id == id);
        }
        
        /// <summary>
        /// Returns a platform given the id of a resource link within the platform.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The platform.</returns>
        public async Task<Platform> GetPlatformByResourceLinkIdAsync(int id)
        {
            return await Platforms
                .Include(p => p.ResourceLinks)
                .SingleOrDefaultAsync(p => p.ResourceLinks.Any(l => l.Id == id));
        }
        
        /// <summary>
        /// Returns a fully populated <see cref="ResourceLink"/>.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The resource link.</returns>
        public async Task<ResourceLink> GetResourceLinkAsync(int id)
        {
            return await ResourceLinks
                .Include(l => l.Tool)
                .SingleOrDefaultAsync(l => l.Id == id);
        }
                
        /// <summary>
        /// Returns a fully populated <see cref="Tool"/>.
        /// </summary>
        /// <param name="id">The tool id.</param>
        /// <returns>The tool.</returns>
        public async Task<Tool> GetToolAsync(int id)
        {
            return await Tools.FindAsync(id);
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
            return await GetUserAsync(id);
        }

        /// <summary>
        /// Returns the lightly populated <see cref="AdvantagePlatformUser"/> (Course and Platform only)
        /// corresponding to the IdentityOptions.ClaimsIdentity.UserIdClaimType claim in the principal.
        /// </summary>
        /// <param name="principal">The principal which contains the user id claim.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserLightAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            var id = GetUserId(principal);
            return await GetUserLightAsync(id);
        }

        /// <summary>
        /// Returns the fully populated <see cref="AdvantagePlatformUser"/>.
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserAsync(string id)
        {
            if (id == null)
            {
                return null;
            }

            return await Users
                .Include(u => u.Course)
                    .ThenInclude(c => c.ResourceLinks)
                        .ThenInclude(l => l.Tool)
                .Include(u => u.Course)
                    .ThenInclude(c => c.GradebookColumns)
                        .ThenInclude(c => c.ResourceLink)
                .Include(u => u.People)
                .Include(u => u.Platform)
                    .ThenInclude(p => p.ResourceLinks)
                        .ThenInclude(l => l.Tool)
                .Include(u => u.Tools)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Returns the lightly populated <see cref="AdvantagePlatformUser"/>
        /// (tenant Course and Platform only).
        /// </summary>
        /// <param name="id">The user id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserLightAsync(string id)
        {
            if (id == null)
            {
                return null;
            }

            return await Users
                .Include(u => u.Course)
                .Include(u => u.Platform)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Returns a user given the user's course id.
        /// </summary>
        /// <param name="id">The course id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserByCourseIdAsync(int id)
        {
            var user = await Users
                .Include(u => u.Course)
                .SingleOrDefaultAsync(u => u.Course.Id == id);

            if (user != null)
            {
                return await GetUserAsync(user.Id);
            }

            return null;
        }

        /// <summary>
        /// Returns a user given the user's platform id.
        /// </summary>
        /// <param name="id">The platform id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserByPlatformIdAsync(int id)
        {
            var user = await Users
                .Include(u => u.Course)
                .SingleOrDefaultAsync(u => u.Platform.Id == id);

            if (user != null)
            {
                return await GetUserAsync(user.Id);
            }

            return null;
        }

        /// <summary>
        /// Returns a user given the id of a resource link within the user's platform or course.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserByResourceLinkIdAsync(int id)
        {
            // Find the course or platform that includes the resource link
            var course = await GetCourseByResourceLinkIdAsync(id);
            if (course != null)
            {
                return await GetUserByCourseIdAsync(course.Id);
            }

            var platform = await GetPlatformByResourceLinkIdAsync(id);
            if (platform != null)
            {
                return await GetUserByPlatformIdAsync(platform.Id);
            }

            return null;
        }

        /// <summary>
        /// Return the user id from the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>The user id.</returns>
        public string GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            // Because this app is using Identity Server, the user id
            // is typically in the sub claim. But when swagger authenticates,
            // the user id is in the nameidentifier claim.
            return principal.FindFirstValue("sub") 
                   ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        #endregion
    }
}
