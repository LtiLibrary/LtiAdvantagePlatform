using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdvantagePlatform;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=advantageplatform.db");
    options.UseOpenIddict();
});

// Sample app: relax password rules.
builder.Services.AddDefaultIdentity<AdvantagePlatformUser>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Use app specific cookie name so AdvantagePlatform and AdvantageTool can both run on
// localhost at the same time without colliding.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "AdvantagePlatform";
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
});

// Identity uses its application cookie scheme for browser auth. OpenIddict maps user
// claims via that cookie when the authorize endpoint is hit.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
    options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
    options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
});

builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/CourseLinks");
        options.Conventions.AuthorizeFolder("/PlatformLinks");
        options.Conventions.AuthorizeFolder("/Tools");
    });

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Swagger / OpenAPI.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advantage Platform",
        Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0",
        Description = "These are the LTI Advantage service endpoints implemented by this sample platform. " +
                      "Click on Authorize and login with the username and password you registered to try the various services."
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, "LtiAdvantage.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }

    options.DocumentFilter<HideRubyRoutesInSwaggerFilter>();

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = Config.LtiScopes.ToDictionary(s => s, s => string.Empty)
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            Config.LtiScopes.ToList()
        }
    });
});

// OpenIddict server / validation.
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("connect/authorize")
            .SetTokenEndpointUris("connect/token")
            .SetUserInfoEndpointUris("connect/userinfo")
            .SetEndSessionEndpointUris("connect/logout");

        options.AllowAuthorizationCodeFlow()
            .AllowImplicitFlow()
            .AllowClientCredentialsFlow()
            .AllowPasswordFlow();

        options.RegisterScopes(Config.LtiScopes.ToArray());

        // Sample only: developer signing/encryption credentials.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Issue id_tokens / access_tokens that downstream tools can verify against
        // /.well-known/openid-configuration. Disabling token encryption keeps the
        // tokens human-readable for testing.
        options.DisableAccessTokenEncryption();

        options.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .DisableTransportSecurityRequirement();

        // Impersonation + LTI claims are added by LtiAdvantageClaimsHandler. We do not
        // call AddImpersonationSupport() because the upstream impersonation handler
        // unconditionally invokes context.HandleRequest(), which short-circuits
        // OpenIddict's response generation for non-authorize sign-ins (e.g. password
        // grant exchanges).
        options.AddLtiAdvantageClaims();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Default scheme so [Authorize] without a scheme uses the application cookie for
// browser pages, and JwtBearer-marked endpoints fall back to OpenIddict validation.
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

builder.Services.AddAuthorization();
builder.Services.AddLtiAdvantagePolicies();
builder.Services.AddTransient<CourseAccessValidator>();
builder.Services.AddScoped<ToolClientManager>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var toolClientManager = scope.ServiceProvider.GetRequiredService<ToolClientManager>();
    await toolClientManager.EnsureSwaggerClientAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "LTI Advantage 1.3");
    options.DocumentTitle = "Advantage Platform - Swagger UI";
    options.OAuthClientId("swagger");
    options.OAuthClientSecret("secret");
});

app.MapRazorPages();
app.MapControllers();

app.Run();
