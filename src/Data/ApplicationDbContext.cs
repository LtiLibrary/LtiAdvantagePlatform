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
        /// Returns a fully populated <see cref="ResourceLink"/>.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns></returns>
        public async Task<ResourceLink> GetResourceLinkAsync(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await ResourceLinks
                .Include(l => l.Tool)
                .SingleOrDefaultAsync(l => l.Id == id);
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

        private string GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.FindFirstValue("sub");
        }

        public async Task<Person> GetPersonAsync(string personId)
        {
            if (!int.TryParse(personId, out var id))
            {
                throw new ArgumentException($"{nameof(personId)} is not an integer.");
            }

            return await People.FindAsync(id);
        }

        public async Task<Course> GetCourseAsync(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await Courses
                .Include(c => c.GradebookColumns)
                    .ThenInclude(c => c.ResourceLink)
                .Include(c => c.ResourceLinks)
                    .ThenInclude(l => l.Tool)
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course> GetCourseByContextIdAsync(string contextId)
        {
            if (!int.TryParse(contextId, out int id))
            {
                throw new ArgumentException($"{nameof(contextId)} is not an integer.");
            }

            return await GetCourseAsync(id);
        }

        public async Task<Course> GetCourseByResourceLinkAsync(int? resourceLinkId)
        {
            if (resourceLinkId == null)
            {
                throw new ArgumentNullException(nameof(resourceLinkId));
            }

            var course = await Courses.SingleOrDefaultAsync(c => c.ResourceLinks.Any(l => l.Id == resourceLinkId));

            return course == null ? null  : await GetCourseAsync(course.Id);
        }

        public async Task<GradebookColumn> GetGradebookColumnByResourceLinkAsync(int? resourceLinkId)
        {
            if (resourceLinkId == null)
            {
                throw new ArgumentNullException(nameof(resourceLinkId));
            }

            return await GradebookColumns.SingleOrDefaultAsync(c => c.ResourceLink.Id == resourceLinkId);
        }

        public async Task<Platform> GetPlatformByResourceLink(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await Platforms.SingleOrDefaultAsync(p => p.ResourceLinks.Any(l => l.Id == id));
        }

        public async Task<AdvantagePlatformUser> GetUserByResourceLink(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Find the course or platform that includes the resource link
            var course = await GetCourseByResourceLinkAsync(id);
            if (course != null)
            {
                return await GetUserByCourse(course.Id);
            }

            var platform = await GetPlatformByResourceLink(id);
            if (platform != null)
            {
                return await GetUserByPlatform(platform.Id);
            }

            return null;
        }

        private async Task<AdvantagePlatformUser> GetUserByCourse(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var user = await Users
                .Include(u => u.Course)
                .SingleOrDefaultAsync(u => u.Course.Id == id);

            if (user != null)
            {
                return await GetUserAsync(user.Id);
            }

            return null;
        }

        private async Task<AdvantagePlatformUser> GetUserByPlatform(int? id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var user = await Users
                .Include(u => u.Course)
                .SingleOrDefaultAsync(u => u.Platform.Id == id);

            if (user != null)
            {
                return await GetUserAsync(user.Id);
            }

            return null;
        }
    }
}
