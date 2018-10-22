using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<AdvantagePlatformUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // In this sample, AdvantagePlatform is responsible for issuing client_ids
        public DbSet<ClientSecret> ClientSecretText { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Platform> Platforms { get; set; }
    }
}
