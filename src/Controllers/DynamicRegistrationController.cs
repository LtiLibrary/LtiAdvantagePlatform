using System;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using LtiAdvantage.AspNetCore.DynamicRegistration;
using LtiAdvantage.DynamicRegistration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AdvantagePlatform.Controllers
{
    /// <summary>
    /// The registration endpoint of LTI Dynamic Registration
    /// (https://www.imsglobal.org/spec/lti-dr/v1p0#tool-registration-request).
    /// A tool POSTs its configuration here after the user started "Register a tool"
    /// (Pages/Tools/Register). The platform creates the tool and its OpenIddict client, and
    /// answers with the client_id and deployment_id it assigned.
    /// </summary>
    public class DynamicRegistrationController : DynamicRegistrationControllerBase
    {
        /// <summary>
        /// Route name of the registration endpoint, set by the library's base class.
        /// </summary>
        public const string RouteName = "lti-dynamic-registration";

        /// <summary>
        /// How long a registration_token can be used.
        /// </summary>
        public static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

        private readonly ApplicationDbContext _context;
        private readonly ToolClientManager _toolClientManager;
        private readonly ToolSigningKeys _toolSigningKeys;
        private readonly ILogger<DynamicRegistrationController> _logger;

        private ToolRegistration _registration;

        public DynamicRegistrationController(
            IWebHostEnvironment env,
            ILogger<DynamicRegistrationController> logger,
            ApplicationDbContext context,
            ToolClientManager toolClientManager,
            ToolSigningKeys toolSigningKeys) : base(env, logger)
        {
            _context = context;
            _toolClientManager = toolClientManager;
            _toolSigningKeys = toolSigningKeys;
            _logger = logger;
        }

        /// <summary>
        /// The registration_token is a random, single-use value this platform issued when the
        /// user started the registration (DR §3.3 leaves its format to the platform). It also
        /// tells which user the tool belongs to.
        /// </summary>
        protected override async Task<ActionResult> AuthorizeRegistrationAsync(string registrationToken)
        {
            if (registrationToken.IsMissing())
            {
                return BearerUnauthorized(null);
            }

            _registration = await _context.ToolRegistrations
                .SingleOrDefaultAsync(r => r.Token == registrationToken);

            if (_registration == null
                || _registration.ToolId != null
                || _registration.CreatedAt + TokenLifetime < DateTime.UtcNow)
            {
                _logger.LogWarning("Unknown, used or expired registration token.");
                return BearerUnauthorized(registrationToken);
            }

            return null;
        }

        protected override async Task<ToolConfiguration> OnRegisterAsync(RegisterToolRequest request)
        {
            var configuration = request.Tool;
            var lti = configuration.LtiToolConfiguration;

            // The platform fetches the tool's keys now: OpenIddict needs a copy for the client,
            // and it shows early that jwks_uri works. Later they are looked up again by kid.
            var keySet = await _toolSigningKeys.GetKeySetAsync(configuration.JwksUri, null, request.CancellationToken);
            if (keySet == null)
            {
                throw new RegistrationRejectedException("invalid_client_metadata",
                    $"No signing keys could be fetched from jwks_uri {configuration.JwksUri}.");
            }

            // A tool can offer a separate URL for deep linking (DR §3.6.1, messages).
            var deepLinking = lti.Messages?.FirstOrDefault(m => m?.Type == Constants.Lti.LtiDeepLinkingRequestMessageType);

            var tool = new Tool
            {
                // The platform chooses the client_id and the deployment_id.
                ClientId = Guid.NewGuid().ToString("N").Substring(0, 16),
                DeploymentId = Guid.NewGuid().ToString("N").Substring(0, 16),
                Name = configuration.ClientName.IfMissingThen(lti.Domain),
                LoginUrl = configuration.InitiateLoginUri,
                LaunchUrl = lti.TargetLinkUri,
                DeepLinkingLaunchUrl = deepLinking == null ? null : deepLinking.TargetLinkUri.IfMissingThen(lti.TargetLinkUri),
                JwksUrl = configuration.JwksUri,
                RedirectUris = string.Join("\r\n", configuration.RedirectUris),
                CustomProperties = lti.CustomParameters == null
                    ? null
                    : string.Join("\r\n", lti.CustomParameters.Select(p => $"{p.Key}={p.Value}"))
            };

            await _toolClientManager.CreateAsync(tool.ClientId, tool.Name, tool.GetAllRedirectUris(), keySet);

            var user = await _context.GetUserLightAsync(_registration.UserId);
            user.Tools.Add(tool);
            await _context.SaveChangesAsync(request.CancellationToken);

            // The token can only be used once.
            _registration.ToolId = tool.Id;
            await _context.SaveChangesAsync(request.CancellationToken);

            _logger.LogInformation("Registered tool {ToolName} with client id {ClientId}.", tool.Name, tool.ClientId);

            // Answer with the tool's configuration plus what the platform assigned (DR §3.6.2).
            configuration.ClientId = tool.ClientId;
            lti.DeploymentId = tool.DeploymentId;

            // Only grant scopes the platform supports. (This sample's clients may request all
            // of them, like clients registered by hand.)
            var requested = (configuration.Scope ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            configuration.Scope = string.Join(" ", requested.Intersect(Config.LtiScopes));

            return configuration;
        }
    }
}
