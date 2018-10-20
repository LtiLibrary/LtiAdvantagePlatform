using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(AdvantagePlatform.Areas.Identity.IdentityHostingStartup))]
namespace AdvantagePlatform.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}