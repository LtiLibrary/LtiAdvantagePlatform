using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdvantagePlatform.Data;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AdvantagePlatform
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<AdvantagePlatformUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options => { options.Cookie.Name = "AdvantagePlatform"; });

            services.AddMvc()
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Clients"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Tools"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Deployments"); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // This app will host the LTI Platform and the Identity Server (Issuer). The Identity Server
            // will 1) sign requests that originate from the Platform and a keyset endpoint so the Tool 
            // (Client) can get the public key to validate the requests, and 2) issue access tokens to
            // the Tool and validate requests that originate from the Tool.

            // This app uses ASP.NET Core Idenity to manage local accounts (see AddDefaultIdentity above).
            // I have added IndentityServer following the instructions here:
            // https://identityserver4.readthedocs.io/en/release/quickstarts/6_aspnet_identity.html
            // https://github.com/IdentityServer/IdentityServer4/issues/2373#issuecomment-398824428
            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                })
                
                // For this test app, I use the DeveloperSigningCredential
                .AddDeveloperSigningCredential()

                // Store Configuration and Operational data in the database
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                            sql => sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                            sql => sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name));

                    // Clean up expired tokens
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })

                // Configure IdentityServer to work with ASP.NET Core Identity
                .AddAspNetIdentity<AdvantagePlatformUser>();


            // Add AddAuthentication and set the default scheme to IdentityConstants.ApplicationScheme
            // so that IdentityServer can find the right ASP.NET Core Identity pages
            // https://github.com/IdentityServer/IdentityServer4/issues/2510#issuecomment-411871543
            services.AddAuthentication(IdentityConstants.ApplicationScheme)

                // Add Bearer authentication to authenticate API calls
                .AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Authority"];
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidAudiences = Config.GetApiResources().Select(a => a.Name)
                    };
                });
        }
        
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                //if (!EnumerableExtensions.Any(context.Clients))
                //{
                //    foreach (var client in Config.GetClients())
                //    {
                //        context.Clients.Add(client.ToEntity());
                //    }
                //    context.SaveChanges();
                //}

                if (!EnumerableExtensions.Any(context.IdentityResources))
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!EnumerableExtensions.Any(context.ApiResources))
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithRedirects("/Error?httpStatusCode={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseIdentityServer();

            app.UseMvc();
        }
    }
}
