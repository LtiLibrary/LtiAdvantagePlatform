using System.Collections.Generic;
using System.Linq;
using AdvantagePlatform.Data;
using AdvantagePlatform.Utility;
using LtiAdvantage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace AdvantagePlatform.Spa
{
    /// <summary>
    /// JSON endpoints for the React front end in ClientApp/.
    ///
    /// The React app only displays data and decides where to show a tool (iframe or new tab).
    /// Everything LTI-specific stays on the server: the API hands React ready-made launch URLs
    /// that point at /OidcLaunch, and the platform signs the id_token at /connect/authorize.
    /// React never sees a private key or builds an LTI message.
    ///
    /// The API uses the same Identity cookie as the Razor Pages. React is served from the same
    /// origin, so the browser sends the cookie automatically. The endpoints are read-only, so
    /// they need no antiforgery token.
    /// </summary>
    public static class SpaApi
    {
        public static void MapSpaApi(this WebApplication app)
        {
            var api = app.MapGroup("/api/spa").RequireAuthorization();

            api.MapGet("/home", async (HttpContext http, ApplicationDbContext db, LinkGenerator links) =>
            {
                var user = await db.GetUserFullAsync(http.User);
                if (user == null)
                {
                    return Results.Unauthorized();
                }

                var people = user.People
                    .OrderBy(p => p.FirstName)
                    .Select(p => new PersonDto(p.Id, $"{p.FirstName} {p.LastName}".Trim(), p.Roles))
                    .ToList();

                // Each launch URL is built per person: the sample lets you launch as the teacher
                // or the student. A real LMS would launch as the signed-in user instead.
                Dictionary<int, string> LaunchUrls(int id, string messageType, int? courseId) =>
                    people.ToDictionary(
                        p => p.Id,
                        p => links.GetPathByPage(http, "/OidcLaunch", values: new
                        {
                            id,
                            messageType,
                            courseId,
                            personId = p.Id
                        }));

                List<LinkDto> Links(IEnumerable<ResourceLink> resourceLinks, int? courseId) =>
                    resourceLinks
                        .OrderBy(l => l.Title)
                        .Select(l => new LinkDto(
                            l.Id,
                            l.Title,
                            l.Description,
                            l.Tool.Name,
                            l.DocumentTarget.IfMissingThen("window"),
                            l.IsProctored,
                            // A proctored assessment starts at the proctoring tool (LtiStartProctoring).
                            LaunchUrls(l.Id, l.IsProctored
                                ? Constants.Lti.LtiStartProctoringRequestMessageType
                                : Constants.Lti.LtiResourceLinkRequestMessageType, courseId)))
                        .ToList();

                // Non-LTI content added with Deep Linking. HTML fragments are left out: showing
                // tool-supplied HTML safely needs a sandboxed iframe, see the Razor UI.
                List<ContentDto> Content(IEnumerable<LinkedContent> content) =>
                    content
                        .Where(c => c.Url.IsPresent())
                        .OrderBy(c => c.Title)
                        .Select(c => new ContentDto(c.Id, c.Type, c.Title.IfMissingThen(c.Url), c.Text, c.Url))
                        .ToList();

                List<DeepLinkToolDto> DeepLinkTools(int? courseId) =>
                    user.Tools
                        .Where(t => t.DeepLinkingLaunchUrl.IsPresent())
                        .OrderBy(t => t.Name)
                        .Select(t => new DeepLinkToolDto(
                            t.Id,
                            t.Name,
                            LaunchUrls(t.Id, Constants.Lti.LtiDeepLinkingRequestMessageType, courseId)))
                        .ToList();

                return Results.Ok(new HomeDto(
                    new ContextDto(
                        user.Platform.Name.IfMissingThen("Platform"),
                        Links(user.Platform.ResourceLinks, null),
                        Content(user.Platform.LinkedContent),
                        DeepLinkTools(null)),
                    new ContextDto(
                        user.Course.Name.IfMissingThen("Course"),
                        Links(user.Course.ResourceLinks, user.Course.Id),
                        Content(user.Course.LinkedContent),
                        DeepLinkTools(user.Course.Id)),
                    people));
            });
        }

        /// <summary>
        /// Serves the built React app (ClientApp → wwwroot/app) for /app and any path below it,
        /// so client-side routes work on refresh. Static assets under /app/assets are served by
        /// UseStaticFiles before this is reached.
        /// </summary>
        public static void MapSpaFallback(this WebApplication app)
        {
            app.MapGet("/app/{**path}", (IWebHostEnvironment env) =>
            {
                var index = env.WebRootFileProvider.GetFileInfo("app/index.html");
                return index.Exists
                    ? Results.File(index.PhysicalPath, "text/html")
                    : Results.Text(
                        "The React app has not been built yet. Run:\n\n" +
                        "    cd src/ClientApp\n    npm install\n    npm run build\n\n" +
                        "Or run `npm run dev` there and open http://localhost:5173/app/ instead.",
                        "text/plain");
            });
        }

        /// <summary>
        /// Origins (besides the platform's own) that may receive the "deep linking finished"
        /// message from /DeepLinks. Used for the Vite dev server, which runs on another port.
        /// </summary>
        public static string[] GetSpaOrigins(this IConfiguration configuration) =>
            configuration.GetSection("ClientApp:AllowedOrigins").Get<string[]>() ?? [];

        public record HomeDto(ContextDto Platform, ContextDto Course, List<PersonDto> People);

        public record ContextDto(string Name, List<LinkDto> Links, List<ContentDto> Content, List<DeepLinkToolDto> DeepLinkTools);

        /// <param name="DocumentTarget">"iframe" or "window": where the tool asked to be shown.</param>
        /// <param name="Proctored">A proctored assessment: the launch URLs start a proctored attempt.</param>
        public record LinkDto(int Id, string Title, string Description, string ToolName, string DocumentTarget, bool Proctored, Dictionary<int, string> LaunchUrls);

        /// <param name="Type">The deep linking content item type: "link", "file" or "image".</param>
        public record ContentDto(int Id, string Type, string Title, string Text, string Url);

        public record DeepLinkToolDto(int Id, string Name, Dictionary<int, string> LaunchUrls);

        public record PersonDto(int Id, string Name, string Roles);
    }
}
