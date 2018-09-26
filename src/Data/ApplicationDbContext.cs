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

        // This platform issues Client IDS
        public DbSet<Client> Clients { get; set; }

        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Tool> Tools { get; set; }

    }
}
