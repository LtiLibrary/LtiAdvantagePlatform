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
    }
}
