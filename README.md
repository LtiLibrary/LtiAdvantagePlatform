# LTI Advantage Platform

A sample LTI 1.3 / LTI Advantage **platform** built with ASP.NET Core. It pairs
with the [LtiAdvantage](https://github.com/LtiLibrary/LtiAdvantage) libraries
and demonstrates how to host the LTI Advantage services from a platform's
perspective. There is a companion sample tool over at
[LtiAdvantageTool](https://github.com/LtiLibrary/LtiAdvantageTool).

## Supported features

- LTI 1.3 Core launch
  - Launch tools with or without a context (i.e. as an assignment or an admin tool), and as a teacher or a student.
- Assignment and Grade Services v2
  - Tools can list line items for a course, get a specific line item, read results, and post scores.
- Names and Role Provisioning Services v2
  - Tools can read course membership.
- Deep Linking 2.0
  - Tools can add LTI links, links, HTML fragments, files and images to the platform or course.
  - The platform shows the tool's `msg` / `errormsg`, and honours the `iframe` or `window` presentation of each item.
- Course Groups Service 1.0
  - Tools can read the course's group sets, groups, and each member's groups.
- Submission Review 1.0
  - Tools mark line items with `submissionReview`; the gradebook then offers "View submission", which launches the tool for that student.
- Dynamic Registration 1.0
  - Tools register themselves: no URLs, client IDs or keys to copy by hand.
- Tool keys from the tool's JWKS URL
  - Cached and picked by `kid`, so tools can rotate their keys. A PEM public key is the fallback.

## Stack

This sample tracks the LtiAdvantage libraries' modernized stack:

- **ASP.NET Core 10** (Razor Pages + MVC controllers).
- **OpenIddict 7.x** as the OpenID Connect / OAuth 2.0 server, replacing the
  earlier dependency on Duende / IdentityServer4.
- **ASP.NET Core Identity** for application user accounts.
- **EF Core 10** with **SQLite** as the default database (cross-platform).
- **LtiAdvantage**, **LtiAdvantage.AspNetCore**, and **LtiAdvantage.Oidc**
  packages for the LTI message types, controller bases, and OpenIddict
  impersonation hook.

The sample takes the same architectural approach as before — each registered
application user is a tenant with their own platform, course, and people — but
the OIDC plumbing is now OpenIddict. See `Program.cs` for how the server, the
LtiAdvantage impersonation handler, and the LTI claims event handler are wired
together; the LTI claims handler appends id_token claims for resource-link and
deep-linking launches.

## Layout

```
/ (Visual Studio solution file)
└── src/
    ├── Program.cs (top-level Hosting + OpenIddict + LtiAdvantage setup)
    ├── Config.cs (LTI scopes registered with OpenIddict)
    ├── Areas/Identity/ (ASP.NET Core Identity user management; edit platform, course, and people)
    ├── ClientApp/ (optional React front end, see "Using a React front end")
    ├── Controllers/
    │   ├── AuthorizationController.cs (OIDC /connect/authorize + /connect/token endpoints)
    │   ├── OpenIdConfigurationController.cs / DynamicRegistrationController.cs (Dynamic Registration)
    │   ├── LineItemsController.cs / ResultsController.cs / ScoresController.cs (AGS v2)
    │   ├── MembershipController.cs (NRPS v2)
    │   └── GroupSetsController.cs / GroupsController.cs (Course Groups)
    ├── Data/ (EF Core entities and migrations)
    ├── Spa/SpaApi.cs (JSON API and /app hosting for the React front end)
    ├── Pages/
    │   ├── DeepLinks.cshtml (receives the deep-linking response)
    │   └── OidcLaunch.cshtml (initiates the LTI 1.3 launch by redirecting to the tool)
    ├── Utility/
    │   ├── LtiAdvantageClaimsHandler.cs (OpenIddict ProcessSignInContext handler that adds LTI claims)
    │   ├── LtiClientAssertionHandlers.cs (accepts the client assertions LTI tools send to /connect/token)
    │   ├── ToolClientManager.cs (creates / updates OpenIddict applications for registered tools)
    │   └── CourseAccessValidator.cs (per-tenant access checks)
    └── wwwroot/ (static website files)
```

## Running locally

Prerequisites:

- .NET 10 SDK. `global.json` pins this.
- A checkout of [LtiAdvantage](https://github.com/LtiLibrary/LtiAdvantage) next
  to this repository. The sample builds against the library source, so the
  folders must sit side by side:

  ```
  some-folder/
  ├── LtiAdvantage/
  └── LtiAdvantagePlatform/
  ```

Steps from the repo root:

```bash
dotnet build
dotnet run --project src/AdvantagePlatform.csproj
```

The app listens on `http://localhost:5099` (use `--launch-profile https` for
`https://localhost:7099`). On first run EF Core creates a SQLite
`advantageplatform.db` in the working directory and seeds the OpenIddict
`swagger` confidential client used by Swagger UI's "Authorize" button.

Visit:

- `/` — register a user and explore the test platform / course / people.
- `/swagger` — try the AGS / NRPS endpoints (use Swagger's Authorize, the
  swagger client uses the password grant).
- `/.well-known/openid-configuration` — OpenID Connect discovery.
- `/.well-known/jwks` — JSON Web Key Set used by tools to verify id_tokens.

## Trying it with the sample tool

Run [LtiAdvantageTool](https://github.com/LtiLibrary/LtiAdvantageTool) at the
same time (it listens on `http://localhost:5072`). The quickest way to connect
the two is [Dynamic Registration](#registering-a-tool-with-dynamic-registration).
To register the tool by hand instead:

1. In the tool, register an admin account and open **Platforms → Add
   platform**. Leave the page open: its "Tool side" section lists the URLs and
   public key the platform needs.
2. In the platform, register an account and open **Tools → Register New
   Tool**. Enter the tool's Login URL, Launch URL, Deep linking URL and JWKS
   URL (or its public key). Note the Client ID, and the Issuer, Authorize URL, Token URL and JWKS
   URL shown on the page.
3. Back in the tool, fill in the platform side with those values and save.
   Copy the Issuer exactly, including the trailing slash
   (`http://localhost:5099/`): it must match the `iss` claim of the id_token
   character for character.
4. In the platform, create a **Course Link** (or **Platform Link**) for the
   tool, or use **Add Deep Links**, then launch it from the home page as the
   teacher or the student.

A launch from a course link also gives the tool access to Assignment and
Grade Services and Names and Role Provisioning. Scores the tool posts show up
in the platform's gradebook.

## Deep Linking

Click **Add Deep Links** on the home page (or **Add content with** in the
React UI). The platform sends an `LtiDeepLinkingRequest` whose
`deep_linking_settings` advertise:

- `accept_types`: `ltiResourceLink`, `link`, `html`, `file`, `image`;
- `accept_presentation_document_targets`: `iframe`, `window`;
- `accept_multiple`: true.

The tool posts its response to `/DeepLinks` (`Pages/DeepLinks.cshtml.cs`),
which checks the signature with the tool's keys and saves each content item:

- `ltiResourceLink` becomes a platform or course link. An `iframe` property
  makes the link open inside the platform page (launched with
  `launch_presentation.document_target` = `iframe`); otherwise it opens in a
  new window. In a course every link gets a gradebook column; a `lineItem` on
  the item sets the column's `label`, `scoreMaximum`, `tag` and `resourceId`.
- `link`, `file` and `image` are shown as a link, a download or an image.
  `html` is shown in a sandboxed iframe, so the tool's HTML never runs in the
  platform's own page.

`msg` and `errormsg` from the response are shown to the user after the
dialog closes (`/DeepLinks` adds them to its `lti-deep-linking-complete`
message); `log` and `errorlog` go to the platform's log.

## Course Groups

Every new account's course has three people (a teacher and two students) and
two group sets: **Project teams** (Team Red and Team Blue, one student each)
and **Study groups** (Everyone). The home page lists them under Members.
(Accounts registered before this feature have no groups; register a new one.)

Launches from a course carry the Course Groups service claim
(`CourseGroupsServiceClaimValueType`) with the two endpoints, which need the
`https://purl.imsglobal.org/spec/lti-gs/scope/contextgroup.readonly` scope:

- `GET /context/{courseId}/groups/sets`: the group sets
  (`Controllers/GroupSetsController.cs`, on `GroupSetsControllerBase`);
- `GET /context/{courseId}/groups[?user_id=…]`: the groups, or only those of
  one user (`Controllers/GroupsController.cs`, on `GroupsControllerBase`);
- `GET /context/{courseId}/membership?groups=true`: Names and Roles with each
  member's `group_enrollments`. The library only lets those through when the
  token also has the groups scope.

Existing tool clients get the new scope at startup
(`ToolClientManager.EnsureScopePermissionsAsync`).

## Submission Review

A tool that can show a student's submission says so on the line item: it
creates or updates the line item (Assignment and Grade Services) with a
`submissionReview` property (`SubmissionReviewProperty`: `label`, optional
`url` and `custom`). The platform stores it on the gradebook column
(`Controllers/LineItemsController.cs`) and returns it when the tool reads the
line item.

In the gradebook, every score in such a column gets a button with the
review's label ("View submission" by default). It launches an
`LtiSubmissionReviewRequest` through the usual `/OidcLaunch` →
`/connect/authorize` flow, as the course's instructor, with:

- `for_user`: the student whose submission to show (`ForUserClaimValueType`);
- the AGS claim with that line item's URL, and the line item's resource link;
- `target_link_uri`: the `submissionReview.url`, or the tool's launch URL;
- the `submissionReview.custom` parameters on top of the link's own.

See `GetSubmissionReviewRequestClaims` in `Utility/LtiAdvantageClaimsHandler.cs`.

## Registering a tool with Dynamic Registration

Instead of copying URLs, client IDs and keys between the two apps, let the
tool register itself ([LTI Dynamic Registration](https://www.imsglobal.org/spec/lti-dr/v1p0)):

1. In the platform open **Tools → Register a Tool**, enter the tool's
   registration URL (from the tool's own UI) and click **Start registration**.
2. The platform opens that URL in an iframe (or a popup, if the tool can't run
   in an iframe) with two query parameters:
   - `openid_configuration`: `http://localhost:5099/.well-known/openid-configuration`
   - `registration_token`: a random, single-use token, valid for an hour.
3. The tool reads the platform's endpoints from the OpenID configuration and
   POSTs its configuration to the registration endpoint,
   `http://localhost:5099/lti/register`, with the token as Bearer token.
4. The platform creates the tool and its OpenIddict client, using the tool's
   `jwks_uri` for its keys, and answers with the `client_id` and
   `deployment_id` it assigned.
5. The tool posts `{ subject: "org.imsglobal.lti.close" }` to the platform's
   window, and the platform shows the new tool.

The pieces, all built on the library:

- `Controllers/OpenIdConfigurationController.cs` (`OpenIdConfigurationControllerBase`)
  serves the OpenID configuration with the LTI platform configuration claim
  (`PlatformOpenIdConfiguration`, `LtiPlatformConfiguration`). OpenIddict's own
  discovery endpoint is turned off in `Program.cs`, so there is one document at
  that address.
- `Controllers/DynamicRegistrationController.cs` (`DynamicRegistrationControllerBase`)
  checks the registration token and turns the submitted `LtiToolConfiguration`
  into a tool. The library validates the request first (required fields,
  https URLs, `private_key_jwt`).
- `Pages/Tools/Register.cshtml` starts the registration and listens for the
  tool's close message.

## Using a React front end

`src/ClientApp` is a small React (Vite + TypeScript) front end for the same
platform, served at `/app`. It shows how an LMS with a single-page-app front
end and an ASP.NET Core back end can host LTI tools. It lists the platform and
course links, launches a tool in an iframe, and adds links with Deep Linking.

Build it once, then run the platform as usual and open
`http://localhost:5099/app/` (or click **React UI** in the menu):

```bash
cd src/ClientApp
npm install
npm run build        # writes to src/wwwroot/app
```

For hot reload, keep the platform running and use `npm run dev` instead, then
open `http://localhost:5173/app/`. The Vite dev server forwards `/api`,
`/OidcLaunch` and the sign-in pages to the platform.

### What React does, and what stays on the server

The LTI 1.3 launch is a chain of browser redirects and form posts that ends
with the platform signing an `id_token`. The signing key must stay on the
server, so React doesn't take part in the launch. It only decides where the
tool appears:

| Step | Where |
| --- | --- |
| List links, tools and people | React, from `GET /api/spa/home` (`Spa/SpaApi.cs`) |
| Start a launch | React sets an `<iframe>`'s `src` to a launch URL from the API (`/OidcLaunch?...`) |
| OIDC login initiation, `/connect/authorize`, signed `id_token` form post | Server, inside the iframe. The same code serves the Razor Pages UI |
| Deep Linking response | The tool posts to `/DeepLinks`, which validates and saves the links, then sends `postMessage` to the parent window |
| Assignment and Grade Services, Names and Role Provisioning | Server only. Tools call these APIs directly with their own access tokens |

The pieces that make this work:

- **The same cookie for the API and the launch.** React is served from the
  platform's own origin, so `fetch("/api/...")` sends the ASP.NET Core
  Identity cookie, and `/connect/authorize` inside the iframe sees the same
  signed-in user. If your React app lives on another origin, put the API and
  the launch endpoints behind one origin (a backend-for-frontend), because the
  authorize endpoint has to know who is signed in.
- **API calls get a 401, not a redirect to the login page.** See
  `ConfigureApplicationCookie` in `Program.cs`.
- **Launch URLs come from the server.** The API returns ready-made URLs, so
  the front end never builds LTI parameters.
- **Deep Linking ends with `postMessage`.** `/DeepLinks` posts
  `{ type: "lti-deep-linking-complete", added, msg, errormsg }` to the parent window, with
  the platform's own origin as target (plus `ClientApp:AllowedOrigins` from
  `appsettings.Development.json` for the Vite dev server). React checks
  `event.origin`, closes the iframe and reloads its data. The Razor Pages UI
  listens for the same message.
- **No `sandbox` on the iframe.** Tools need scripts, forms and their own
  cookies. Offer "open in a new tab" as a fallback for tools that can't run
  in an iframe (for example when the browser blocks their cookies as
  third-party cookies).

A real LMS would launch as the signed-in user. This sample lets you pick the
teacher or the student, so the API returns one launch URL per person.

## How the platform handles tools

Tools are configured under **Tools → Register New Tool**. The sample manages
an OpenIddict application for each tool: a confidential client with the
launch URL as redirect URI. The tool signs the client assertion it sends to
`/connect/token` (LTI's JWT client credentials grant) and its deep linking
responses with its private key, so the platform needs the tool's public key.

### Tool keys from a JWKS URL

Enter the tool's **Tool JWKS URL** (the sample tool publishes its keys at
`http://localhost:5072/jwks/{platformId}`). The platform fetches the key set
with the library's `JwksCache` (`Utility/ToolSigningKeys.cs`) and picks the key
by the token's `kid`. When a token names a `kid` the cache doesn't know, the
cache fetches the set again, so the tool can rotate its key without anyone
updating the platform. That covers both places the platform checks a tool's
signature:

- deep linking responses (`Pages/DeepLinks.cshtml.cs`);
- client assertions at `/connect/token`. OpenIddict normally checks them
  against the key set stored on the client's application;
  `ToolJwksClientAssertionHandler` (`Utility/LtiClientAssertionHandlers.cs`)
  hands it the keys from the JWKS URL instead. The stored key set is a copy
  taken when the tool is saved, used only if the JWKS URL can't be reached.

A pasted PEM public key still works as a fallback for tools without a JWKS URL.

[LtiAdvantage](https://github.com/LtiLibrary/LtiAdvantage) provides the LTI
message classes, the AGS / NRPS controller base classes
(`LtiAdvantage.AspNetCore.*`), and the OpenIddict impersonation handler
(`LtiAdvantage.Oidc`).

## License

MIT, see [LICENSE](LICENSE).
