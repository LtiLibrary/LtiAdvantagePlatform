using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Validation;
using LtiAdvantage.IdentityServer4.Validation;
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

            // Use app specific cookie name so both AdvantagePlatform and AdvantageTool can run at the same time.
            services.ConfigureApplicationCookie(options => { options.Cookie.Name = "AdvantagePlatform"; });

			// Some pages require authorization.
            services.AddMvc()
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/CourseLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/PlatformLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Tools"); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// Add Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title= "Advantage Platform", 
                    Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    Description = "These are the LTI Advantage service endpoints implemented by this sample platform. " 
                                  + "Click on Authorize and login with the username and password you registered to try the various services."
                });
                options.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "LtiAdvantage.xml"));
                options.SwaggerGeneratorOptions.DocumentFilters = new List<IDocumentFilter>
                    {new HideRubyRoutesInSwaggerFilter()};
                options.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    TokenUrl = "/connect/token",
                    Type = "oauth2",
                    Flow = "password",
                    Scopes = Config.LtiScopes.ToDictionary(s => s, s => "")
                });
                options.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "oauth2", Config.LtiScopes }
                });
            });

			// Add an HttpClientFactory
            services.AddHttpClient();

            // Add Identity Server configured to support LTI Advantage needs
            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                })

                // This is not appropriate for production
                .AddDeveloperSigningCredential()

                // In LTI Advantage world, client credentials are signed JWTs
                .AddSecretParser<JwtBearerClientAssertionSecretParser>()
                .AddSecretValidator<PrivatePemKeyJwtSecretValidator>()

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

                // Custom profile service to add LTI Advantage claims to id_token
                .AddProfileService<LtiAdvantageProfileService>()

                // Allow the ASP.NET user to impersonate a Platform user (e.g. a student in the course)
                .AddImpersonationSupport();

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
