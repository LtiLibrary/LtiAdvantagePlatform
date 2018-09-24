using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This platform issues Client IDS
        public DbSet<Client> Clients { get; set; }

        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<KeySet> KeySets { get; set; }
        public DbSet<Tool> Tools { get; set; }

    }
}
