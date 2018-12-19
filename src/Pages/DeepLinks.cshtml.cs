using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using IdentityServer4.EntityFramework.Interfaces;
using LtiAdvantage;
using LtiAdvantage.DeepLinking;
using LtiAdvantage.IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvantagePlatform.Pages
{
    [IgnoreAntiforgeryToken]
    public class DeepLinksModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfigurationDbContext _identityContext;

        public DeepLinksModel(ApplicationDbContext context, IConfigurationDbContext identityContext)
        {
            _context = context;
            _identityContext = identityContext;
        }

        [BindProperty(Name = "id_token")]
        public string IdToken { get; set; }

        public async Task<IActionResult> OnPost(int platformId, int? courseId = null)
        {
            if (IdToken.IsMissing())
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Missing data",
                    Detail = $"{nameof(IdToken)} is missing."
                });
            }

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(IdToken);

            var messageType = token.Claims.SingleOrDefault(c => c.Type == Constants.LtiClaims.MessageType)?.Value;
            if (messageType != Constants.Lti.LtiDeepLinkingResponseMessageType)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Incorrect message format",
                    Detail = $"Expecting {Constants.Lti.LtiDeepLinkingResponseMessageType}, but found {messageType}."
                });
            }

            var client = await _identityContext.Clients.SingleOrDefaultAsync(c => c.ClientId == token.Issuer);
            var tool = await _context.Tools.SingleOrDefaultAsync(t => t.IdentityServerClientId == client.Id);
            var platform = await _context.GetPlatformAsync(platformId);
            var course = courseId.HasValue ? await _context.GetCourseAsync(courseId.Value) : null;

            var ltiRequest = new LtiDeepLinkingResponse(token.Payload);
            var contentItems = ltiRequest.ContentItems;
            if (contentItems != null)
            {
                foreach (var contentItem in contentItems)
                {
                    var resourceLink = new ResourceLink
                    {
                        CustomProperties = contentItem.Custom.ToDatabaseString(),
                        Description = contentItem.Text,
                        Title = contentItem.Title,
                        Tool = tool
                    };

                    if (course == null)
                    {
                        platform.ResourceLinks.Add(resourceLink);
                    }
                    else
                    {
                        course.ResourceLinks.Add(resourceLink);
                        course.GradebookColumns.Add(new GradebookColumn
                        {
                            Label = resourceLink.Title,
                            ResourceLink = resourceLink,
                            ScoreMaximum = 100,
                            Tag = "Deep Link"
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Redirect("/");
        }
    }
}