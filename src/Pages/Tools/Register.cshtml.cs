using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AdvantagePlatform.Controllers;
using AdvantagePlatform.Data;
using LtiAdvantage;
using LtiAdvantage.DynamicRegistration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages.Tools
{
    /// <summary>
    /// Starts LTI Dynamic Registration (https://www.imsglobal.org/spec/lti-dr/v1p0).
    /// The platform opens the tool's registration initiation URL with two query parameters:
    /// openid_configuration (where the tool reads the platform's endpoints) and
    /// registration_token (which the tool sends back to the registration endpoint).
    /// When the tool is done it posts an "org.imsglobal.lti.close" message to this page.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required]
        [Display(Name = "Registration URL", Description = "The tool's registration initiation URL.")]
        public string InitiationUrl { get; set; }

        /// <summary>
        /// The URL the iframe (or popup) opens, once the registration has started.
        /// </summary>
        public string StartUrl { get; private set; }

        /// <summary>
        /// The origin of the tool's page. Only its close message is accepted.
        /// </summary>
        public string ToolOrigin { get; private set; }

        public int? RegistrationId { get; private set; }

        public string Message { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The library's URL rule: https, or http for localhost only.
            if (!RegistrationUrl.IsAllowed(InitiationUrl))
            {
                ModelState.AddModelError(nameof(InitiationUrl), "Enter an absolute https URL (http is allowed for localhost).");
                return Page();
            }

            var registration = new ToolRegistration
            {
                Token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32)),
                UserId = _context.GetUserId(User),
                InitiationUrl = InitiationUrl,
                CreatedAt = DateTime.UtcNow
            };
            _context.ToolRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            StartUrl = QueryHelpers.AddQueryString(InitiationUrl, new Dictionary<string, string>
            {
                ["openid_configuration"] = Url.RouteUrl(
                    Constants.ServiceEndpoints.OpenIdConnect.OpenIdConfigurationService,
                    values: null, protocol: Request.Scheme, host: Request.Host.Value),
                ["registration_token"] = registration.Token
            });
            ToolOrigin = new Uri(InitiationUrl).GetLeftPart(UriPartial.Authority);
            RegistrationId = registration.Id;

            return Page();
        }

        /// <summary>
        /// Called by the page after the tool posted its close message.
        /// </summary>
        public async Task<IActionResult> OnGetCompleteAsync(int id)
        {
            var userId = _context.GetUserId(User);
            var registration = await _context.ToolRegistrations
                .SingleOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (registration?.ToolId != null)
            {
                return RedirectToPage("./Details", new { id = registration.ToolId });
            }

            Message = "The tool closed the registration without registering.";
            return Page();
        }
    }
}
