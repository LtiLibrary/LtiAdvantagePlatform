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
  - Tools can add LTI links to the platform or course.

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
    ├── Controllers/
    │   ├── AuthorizationController.cs (OIDC /connect/authorize + /connect/token endpoints)
    │   ├── LineItemsController.cs / ResultsController.cs / ScoresController.cs (AGS v2)
    │   └── MembershipController.cs (NRPS v2)
    ├── Data/ (EF Core entities and migrations)
    ├── Pages/
    │   ├── DeepLinks.cshtml (receives the deep-linking response)
    │   └── OidcLaunch.cshtml (initiates the LTI 1.3 launch by redirecting to the tool)
    ├── Utility/
    │   ├── LtiAdvantageClaimsHandler.cs (OpenIddict ProcessSignInContext handler that adds LTI claims)
    │   ├── ToolClientManager.cs (creates / updates OpenIddict applications for registered tools)
    │   └── CourseAccessValidator.cs (per-tenant access checks)
    └── wwwroot/ (static website files)
```

## Running locally

Prerequisites:

- .NET 10 SDK. `global.json` pins this. The LtiAdvantage libraries multi-target
  net8.0 and net10.0; this sample targets net10.0.

Steps from the repo root:

```bash
dotnet build
dotnet run --project src/AdvantagePlatform.csproj
```

The app listens on the URL configured in `Properties/launchSettings.json`
(default `http://localhost:5099`). On first run EF Core creates a SQLite
`advantageplatform.db` in the working directory and seeds the OpenIddict
`swagger` confidential client used by Swagger UI's "Authorize" button.

Visit:

- `/` — register a user and explore the test platform / course / people.
- `/swagger` — try the AGS / NRPS endpoints (use Swagger's Authorize, the
  swagger client uses the password grant).
- `/.well-known/openid-configuration` — OpenID Connect discovery.
- `/.well-known/jwks` — JSON Web Key Set used by tools to verify id_tokens.

## Reference layout for a tool

Tools are configured under **Tools → Create**. The sample manages an
OpenIddict application for each tool (client_id, redirect URI, allowed
grants). The tool's PEM-encoded RSA public key is stored on the `Tool`
entity and used to verify deep-linking responses.

[LtiAdvantage](https://github.com/LtiLibrary/LtiAdvantage) provides the LTI
message classes, the AGS / NRPS controller base classes
(`LtiAdvantage.AspNetCore.*`), and the OpenIddict impersonation handler
(`LtiAdvantage.Oidc`).

## License

MIT, see [LICENSE](LICENSE).
