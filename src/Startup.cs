using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AdvantagePlatform.Areas.Identity.Pages.Account.Manage;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using LtiAdvantage.AssignmentGradeServices;
using LtiAdvantage.IdentityServer4.Validation;
using LtiAdvantage.Lti;
using LtiAdvantage.NamesRoleProvisioningService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Client = IdentityServer4.EntityFramework.Entities.Client;

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

            services.AddDefaultIdentity<AdvantagePlatformUser>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 4;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Use app specific cookie name so both AdvantagePlatform and AdvantageTool can run at the same time
            services.ConfigureApplicationCookie(options => { options.Cookie.Name = "AdvantagePlatform"; });

            services.AddMvc()
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/CourseLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/PlatformLinks"); })
                .AddRazorPagesOptions(options => { options.Conventions.AuthorizeFolder("/Tools"); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // This app will host the LTI Platform and the Identity Server (Issuer). The Identity Server
            // will 1) sign requests that originate from the Platform and a keyset endpoint so the Tool 
            // (Client) can get the public key to validate the requests, and 2) issue access tokens to
            // the Tool and validate requests that originate from the Tool.

            // This app uses ASP.NET Core Identity to manage local accounts (see AddDefaultIdentity above).
            // I have added IdentityServer following the instructions here:
            // https://identityserver4.readthedocs.io/en/release/quickstarts/6_aspnet_identity.html
            // https://github.com/IdentityServer/IdentityServer4/issues/2373#issuecomment-398824428
            services.AddTransient<IProfileService, LtiAdvantageProfileService>();
            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                })

                // For this test app, I use the DeveloperSigningCredential
                .AddDeveloperSigningCredential()

                // Add JWT client credentials validation
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
                .AddProfileService<LtiAdvantageProfileService>();


            // Add AddAuthentication and set the default scheme to IdentityConstants.ApplicationScheme
            // so that IdentityServer can find the right ASP.NET Core Identity pages
            // https://github.com/IdentityServer/IdentityServer4/issues/2510#issuecomment-411871543
            services.AddAuthentication(IdentityConstants.ApplicationScheme)

                // Add Bearer authentication to authenticate API calls
                .AddJwtBearer(options =>
                {
                    // The JwtBearer authentication handler will use the discovery endpoint
                    // of the authorization server to find the JWKS endpoint, to find the
                    // key to validate the JwtBearer token. Don't forget to define the
                    // base address of your Identity Server here. If you don't, you'll get
                    // "Unauthorized" errors when you call an API that is protected by a
                    // JwtBearer token.
                    options.Authority = Configuration["Authority"];

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidAudiences = Config.GetApiResources().Select(a => a.Name)
                    };
                });

            // Add LTI Advantage service authorization policies
            services.AddLtiAdvantagePolicies();

            services.AddHttpClient();
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

            //app.UseStatusCodePagesWithRedirects("/Error?httpStatusCode={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseIdentityServer();

            app.UseMvc();
        }

        /// <summary>
        /// Configure the Identity Server.
        /// </summary>
        /// <param name="app"></param>
        private static void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                // These are for login/logout of UI
                if (!EnumerableExtensions.Any(context.IdentityResources))
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                // These are for protecting APIs
                foreach (var resource in Config.GetApiResources())
                {
                    if (!context.ApiResources.Any(r => r.Name == resource.Name))
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                }
                context.SaveChanges();
            }
        }
    }

    public class LtiAdvantageProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LtiAdvantageProfileService> _logger;

        public LtiAdvantageProfileService(
            ApplicationDbContext context,
            IConfigurationDbContext identityContext,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LtiAdvantageProfileService> logger)
        {
            _context = context;
            _identityContext = identityContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            if (context.ValidatedRequest is ValidatedAuthorizeRequest request)
            {
                _logger.LogInformation($"Starting {nameof(GetProfileDataAsync)}.");

                var ltiMessageHint = request.Raw["lti_message_hint"];
                if (!int.TryParse(ltiMessageHint, out var resourceLinkId))
                {
                    _logger.LogError("lti_message_hint is not an int.");
                    return;
                }

                var resourceLink = await _context.GetResourceLinkAsync(resourceLinkId);
                if (resourceLink == null)
                {
                    _logger.LogError($"Cannot find resource link [{resourceLinkId}].");
                    return;
                }

                var tool = resourceLink.Tool;
                if (tool == null)
                {
                    _logger.LogError($"Cannot find tool.");
                    return;
                }

                var person = await _context.GetPersonAsync(request.LoginHint);
                if (person == null)
                {
                    _logger.LogError($"Cannot find person [{request.LoginHint}].");
                    return;
                }

                var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);
                if (client == null)
                {
                    _logger.LogError($"Cannot find client [{tool.IdentityServerClientId}].");
                    return;
                }

                var course = await _context.GetCourseByResourceLink(resourceLink.Id);

                var user = await _context.GetUserByResourceLink(resourceLink.Id);
                if (user == null)
                {
                    _logger.LogError($"Cannot find user.");
                    return;
                }

                context.IssuedClaims = GetLtiClaimsAsync(resourceLink, tool, client, person, course, user.Platform);
            }
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.FromResult(0);
        }

        private List<Claim> GetLtiClaimsAsync(
            ResourceLink resourceLink,
            Tool tool,
            Client client,
            Person person,
            Course course,
            Platform platform)
        {
            var httpRequest = _httpContextAccessor.HttpContext.Request;

            var request = new LtiResourceLinkRequest
            {
                Audiences = new[] { client.ClientId },
                DeploymentId = tool.DeploymentId,
                FamilyName = person.LastName,
                GivenName = person.FirstName,
                LaunchPresentation = new LaunchPresentationClaimValueType
                {
                    DocumentTarget = DocumentTarget.Iframe,
                    Locale = CultureInfo.CurrentUICulture.Name,
                    ReturnUrl = $"{httpRequest.Scheme}://{httpRequest.Host}"
                },
                Lis = new LisClaimValueType
                {
                    PersonSourcedId = person.SisId,
                    CourseSectionSourcedId = course?.SisId
                },
                Nonce = LtiResourceLinkRequest.GenerateCryptographicNonce(),
                Platform = new PlatformClaimValueType
                {
                    ContactEmail = platform.ContactEmail,
                    Description = platform.Description,
                    Guid = platform.Id,
                    Name = platform.Name,
                    ProductFamilyCode = platform.ProductFamilyCode,
                    Url = platform.Url,
                    Version = platform.Version
                },
                ResourceLink = new ResourceLinkClaimValueType
                {
                    Id = resourceLink.Id.ToString(),
                    Title = resourceLink.Title
                },
                UserId = person.Id
            };

            // Add the context if the launch is from a course
            // (e.g. an assignment). Leave it blank if the launch
            // is from outside of a course (e.g. a system menu).
            if (course != null)
            {
                request.Context = new ContextClaimValueType
                {
                    Id = course.Id,
                    Title = course.Name,
                    Type = new[] { ContextType.CourseSection }
                };

                // Only include context roles if the launch includes
                // a context.
                request.Roles = PeopleModel.ParsePersonRoles(person.Roles);

                // Only include the Assignment and Grade Services claim if the launch includes a context.
                request.AssignmentGradeServices = new AssignmentGradeServicesClaimValueType
                {
                    Scope = new List<string>
                    {
                        LtiAdvantage.Constants.LtiScopes.AgsLineItem
                    },
                    //LineItem = Url.RouteUrl(LtiAdvantage.Constants.ServiceEndpoints.AgsLineItemService,
                    //    new { contextId = course.Id, id = resourceLink.Id.ToString() },
                    //    "https",
                    //    Request.Host.ToString()),
                    //LineItems = Url.RouteUrl(LtiAdvantage.Constants.ServiceEndpoints.AgsLineItemService,
                    //    new { contextId = course.Id },
                    //    "https",
                    //    Request.Host.ToString())
                };

                // Only include Names and Role Provisioning Service claim if the launch includes a context.
                request.NamesRoleService = new NamesRoleServiceClaimValueType
                {
                    //ContextMembershipUrl = 
                    //    Url.RouteUrl(Constants.ServiceEndpoints.NrpsMembershipService, 
                    //        new { contextId = course.Id },
                    //        "https",
                    //        Request.Host.ToString())
                };
            }
            else
            {
                var roles = PeopleModel.ParsePersonRoles(person.Roles);
                request.Roles = roles.Where(r => !r.ToString().StartsWith("Context")).ToArray();
            }

            // Collect custom properties
            tool.CustomProperties.TryConvertToDictionary(out var custom);
            if (resourceLink.CustomProperties.TryConvertToDictionary(out var linkDictionary))
            {
                foreach (var property in linkDictionary)
                {
                    if (custom.ContainsKey(property.Key))
                    {
                        custom[property.Key] = property.Value;
                    }
                    else
                    {
                        custom.Add(property.Key, property.Value);
                    }
                }
            }

            // Prepare for custom property substitutions
            var substitutions = new CustomPropertySubstitutions
            {
                LtiUser = new LtiUser
                {
                    Username = person.Username
                }
            };

            request.Custom = substitutions.ReplaceCustomPropertyValues(custom);

            return new List<Claim>(request.Claims);
        }
    }
}
