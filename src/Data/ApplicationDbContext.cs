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

        #region Convenience methods

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
        /// <returns>The resource link.</returns>
        public async Task<ResourceLink> GetResourceLinkAsync(int id)
        {
            return await ResourceLinks
                .Include(l => l.Tool)
                .SingleOrDefaultAsync(l => l.Id == id);
        }

        /// <summary>
        /// Returns a resource linke given the id in string format such as from json.
        /// </summary>
        /// <param name="resourceLinkId">The resource link id.</param>
        /// <returns>The resource link.</returns>
        public async Task<ResourceLink> GetResourceLinkAsync(string resourceLinkId)
        {
            if (!int.TryParse(resourceLinkId, out var id))
            {
                throw new ArgumentException($"{nameof(resourceLinkId)} is not an integer.");
            }

            return await GetResourceLinkAsync(id);
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

        /// <summary>
        /// Return the user id from the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">The principal.</param>
        /// <returns>The user id.</returns>
        private string GetUserId(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            // Because this app is using Identity Server, the user id
            // is in the sub claim.
            return principal.FindFirstValue("sub");
        }

        /// <summary>
        /// Return a person given the person id in string format, such as from json.
        /// </summary>
        /// <param name="personId">The person id.</param>
        /// <returns>The person.</returns>
        public async Task<Person> GetPersonAsync(string personId)
        {
            if (!int.TryParse(personId, out var id))
            {
                throw new ArgumentException($"{nameof(personId)} is not an integer.");
            }

            return await People.FindAsync(id);
        }

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
        /// Return a course given the course id in string format such as from Json.
        /// </summary>
        /// <param name="contextId">The course id.</param>
        /// <returns>The course.</returns>
        public async Task<Course> GetCourseByContextIdAsync(string contextId)
        {
            if (!int.TryParse(contextId, out var id))
            {
                throw new ArgumentException($"{nameof(contextId)} is not an integer.");
            }

            return await GetCourseAsync(id);
        }

        /// <summary>
        /// Return a course given the id of a resource link within the course.
        /// </summary>
        /// <param name="resourceLinkId">The resource link id.</param>
        /// <returns>The course.</returns>
        public async Task<Course> GetCourseByResourceLinkAsync(int resourceLinkId)
        {
            var course = await Courses.SingleOrDefaultAsync(c => c.ResourceLinks.Any(l => l.Id == resourceLinkId));

            return course == null ? null  : await GetCourseAsync(course.Id);
        }

        /// <summary>
        /// Returns a gradebook column.
        /// </summary>
        /// <param name="id">The gradebook column id.</param>
        /// <returns>The gradebook column.</returns>
        public async Task<GradebookColumn> GetGradebookColumnAsync(int id)
        {
            return await GradebookColumns.FindAsync(id);
        }

        /// <summary>
        /// Returns a gradebook column given the column id in string format, such as from json.
        /// </summary>
        /// <param name="columnId">The gradebook column id.</param>
        /// <returns>The gradebook column.</returns>
        public async Task<GradebookColumn> GetGradebookColumnAsync(string columnId)
        {
            if (!int.TryParse(columnId, out var id))
            {
                throw new ArgumentException($"{nameof(columnId)} is not an integer.");
            }

            return await GetGradebookColumnAsync(id);
        }

        /// <summary>
        /// Returns the gradebook column for a resource link if there is exactly one. Otherwise
        /// returns null.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The gradebook column.</returns>
        public async Task<GradebookColumn> GetGradebookColumnByResourceLinkAsync(int id)
        {
            var gradebooksColumns = await GradebookColumns.Where(c => c.ResourceLink.Id == id).ToListAsync();
            return gradebooksColumns.Count() == 1 ? gradebooksColumns[0] : null;
        }

        /// <summary>
        /// Returns a platform given the id of a resource link within the platform.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The platform.</returns>
        public async Task<Platform> GetPlatformByResourceLinkAsync(int id)
        {
            return await Platforms.SingleOrDefaultAsync(p => p.ResourceLinks.Any(l => l.Id == id));
        }

        /// <summary>
        /// Returns a user given the id of a resource link within the user's platform or course.
        /// </summary>
        /// <param name="id">The resource link id.</param>
        /// <returns>The user.</returns>
        public async Task<AdvantagePlatformUser> GetUserByResourceLinkAsync(int id)
        {
            // Find the course or platform that includes the resource link
            var course = await GetCourseByResourceLinkAsync(id);
            if (course != null)
            {
                return await GetUserByCourseAsync(course.Id);
            }

            var platform = await GetPlatformByResourceLinkAsync(id);
            if (platform != null)
            {
                return await GetUserByPlatformAsync(platform.Id);
            }

            return null;
        }

        /// <summary>
        /// Returns a user given the user's course id.
        /// </summary>
        /// <param name="id">The course id.</param>
        /// <returns>The user.</returns>
        private async Task<AdvantagePlatformUser> GetUserByCourseAsync(int id)
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
        private async Task<AdvantagePlatformUser> GetUserByPlatformAsync(int id)
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

        #endregion
    }
}
