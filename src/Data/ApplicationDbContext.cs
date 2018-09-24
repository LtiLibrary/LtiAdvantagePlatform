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
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        // This platform issues Client IDS
        public DbSet<Client> Clients { get; set; }

        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<KeySet> KeySets { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Tool> Tools { get; set; }

    }
}
