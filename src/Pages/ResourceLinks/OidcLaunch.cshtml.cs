using System;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using IdentityModel.Client;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Pages.ResourceLinks
{
    public class OidcLaunchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;
        private readonly ILogger<OidcLaunchModel> _logger;

        public OidcLaunchModel(
            ApplicationDbContext context, 
            IConfigurationDbContext identityContext,
            ILogger<OidcLaunchModel> logger)
        {
            _context = context;
            _identityContext = identityContext;
            _logger = logger;
        }

        /// <summary>
        /// Initiate from tool to platform.
        /// </summary>
        /// <param name="id">The <see cref="ResourceLink"/>The resource link id.</param>
        /// <param name="personId">The user id to login.</param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(int? id, string personId)
        {
            if (id == null)
            {
                _logger.LogError(new ArgumentNullException(nameof(id)), "Missing resource link id.");
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                _logger.LogError(new ArgumentNullException(nameof(personId)), "Missing user id.");
                return BadRequest();
            }

            var resourceLink = await _context.GetResourceLinkAsync(id);
            if (resourceLink == null)
            {
                _logger.LogError("Resource link not found.");
                return BadRequest();
            }

            var tool = resourceLink.Tool;
            if (tool == null)
            {
                _logger.LogError("Resource link does not have a tool defined.");
                return BadRequest();
            }

            var client = await _identityContext.Clients.FindAsync(tool.IdentityServerClientId);
            if (client == null)
            {
                _logger.LogError("Client not found");
                return BadRequest();
            }

            // Send request to tool's endpoint to initiate login
            var ru = new RequestUrl(tool.LoginUrl);
            var url = ru.Create(new
            {
                // The issuer identifier for the platform
                iss = Request.HttpContext.GetIdentityServerIssuerUri(),
                // The audience identifier for the client
                aud = client.ClientId,
                // The platform identifier for the user to login
                login_hint = personId,
                // The endpoint to be executed at the end of the OIDC authentication flow
                target_link_uri = tool.LaunchUrl,
                // The identifier of the LtiResourceLink message (or the deep link message, etc)
                lti_message_hint = resourceLink.Id.ToString()
            });

            _logger.LogInformation($"Launching {resourceLink.Title} using {url}");
            return Redirect(url);
        }
    }
}