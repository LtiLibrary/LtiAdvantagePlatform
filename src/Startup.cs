using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

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

			// This is a sample app. Use simple passwords.
            services.AddDefaultIdentity<AdvantagePlatformUser>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Use app specific cookie name so both AdvantagePlatform and AdvantageTool can run
            // on localhost at the same time.
            services.ConfigureApplicationCookie(options => { options.Cookie.Name = "AdvantagePlatform"; });

			// Some pages require authorization.
            services.AddMvc()
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/CourseLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/PlatformLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Tools"); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Add Swagger tools using Swashbuckle.AspNetCore
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title= "Advantage Platform", 
                    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    Description = "These are the LTI Advantage service endpoints implemented by this sample platform. " 
                                  + "Click on Authorize and login with the username and password you registered to try the various services."
                });

                // This will include the xml-doc comments for each endpoint in the Swagger UI
                options.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "LtiAdvantage.xml"));

                // Ruby controllers accept a format extension (i.e. ".json"). All the controllers
                // in this sample platform have two routes for each endpoint, one with and one without
                // the format extension. This filter will remove the route with the format extension.
                options.SwaggerGeneratorOptions.DocumentFilters = new List<IDocumentFilter>
                    {new HideRubyRoutesInSwaggerFilter()};

                // All the controllers in this sample platform require authorization. This 
                // SecurityDefinition will use validate the user is registered, then request
                // an access token from the TokenUrl.
                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    TokenUrl = "/connect/token",
                    Type = "oauth2",
                    Flow = "password",
                    Scopes = Config.LtiScopes.ToDictionary(s => s, s => "")
                });

                // All the controllers in this sample require specific scopes. This 
                // SecurityRequirement will allow the user to select the scopes they
                // want to test with.
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", Config.LtiScopes }
                });
            });

            // Add Identity Server configured to support LTI Advantage needs
            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                })

                // This is not appropriate for production
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
                    options.EnableTokenCleanup = false;
                    options.TokenCleanupInterval = 30;
                })

                // Configure IdentityServer to work with ASP.NET Core Identity
                .AddAspNetIdentity<AdvantagePlatformUser>()

                // Allow the application user to impersonate a Platform member (e.g. a student in the course)
                .AddImpersonationSupport()

                // Custom profile service to add LTI Advantage claims to id_token
                .AddProfileService<LtiAdvantageProfileService>()

                // Adds support for client authentication using JWT bearer assertions
                // where token is signed by a private key stored in PEM format. 
                .AddLtiJwtBearerClientAuthentication();

            // Add authentication and set the default scheme to IdentityConstants.ApplicationScheme
            // so that IdentityServer can find the right ASP.NET Core Identity pages
            // https://github.com/IdentityServer/IdentityServer4/issues/2510#issuecomment-411871543
            services.AddAuthentication(IdentityConstants.ApplicationScheme)

                // Add Bearer authentication to authenticate API calls
                .AddIdentityServerAuthentication(options =>
                {
                    // The JwtBearer authentication handler will use the discovery endpoint
                    // of the authorization server to find the JWKS endpoint, to find the
                    // key to validate the JwtBearer token. Don't forget to define the
                    // base address of your Identity Server here. If you don't, you'll get
                    // "Unauthorized" errors when you call an API that is protected by a
                    // JwtBearer token.
                    options.Authority = Configuration["Authority"];
                });

            // Add LTI Advantage service authorization policies that enforce API scopes
            services.AddLtiAdvantagePolicies();

            // Each app user is a tenant with their own platform, course, and people. This
            // course access validator is used by the controllers to make sure each tenant
            // can only access their own course.
            services.AddTransient<CourseAccessValidator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
			// Configure Identity Server
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

			// Fire up Identity Server (replaces app.UseAuthentication())
            app.UseIdentityServer();

            // Fire up Swagger and Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "LTI Advantage 1.3");
                options.DocumentTitle = "Advantage Platform - Swagger UI";
                options.OAuthClientId("swagger");
                options.OAuthClientSecret("secret");
            });

            app.UseMvc();
        }

        /// <summary>
        /// Configure Identity Server.
        /// </summary>
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!EnumerableExtensions.Any(context.Clients))
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }

                    context.SaveChanges();
                }

                // Define the identity resources that can be requested.
                if (!EnumerableExtensions.Any(context.IdentityResources))
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }

                    context.SaveChanges();
                }

                // Define the API's that will be protected.
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
    }
}
