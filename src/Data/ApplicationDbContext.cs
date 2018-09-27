using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<AdvantagePlatformUser>
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        // In this sample, AdvantagePlatform is responsible for issuing client_ids
        public DbSet<Client> Clients { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Tool> Tools { get; set; }

    }
}
