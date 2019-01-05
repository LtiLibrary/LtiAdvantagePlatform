# LTI Advantage Platform

Sample LTI Advantage Platform using .NET Core 2.1. So that it has a purpose (other than as a reference code sample), you can use it to [test](https://advantageplatform.azurewebsites.net/) LTI 1.3 Tools. There is a sample LTI Advantage Tool project over [here](https://github.com/andyfmiller/LtiAdvantageTool).

## Supported Features

- LTI 1.3 Core (launch)
   - Launch tools with or without a context (i.e. as an assignment or an admin tool), and as a teacher or a student.
- Assignment and Grade Services v2
   - Tools can get all line items for a course, get a specific line item, and get results from the platform; and they can post scores to the platform.
- Names and Role Provisioning Services v2
   - Tools can get course membership from the platform.
- Deep Linking
   - Tools can add LTI links to the platform or course.

## Dependencies

This web application is based on the current ASP.NET Core Web Application template for Razor Pages.
Most of the application is dedicated to managing users of the app and the tools they will test. Because
LTI Advantage uses OpenID Connect and OAuth to handle authorization and authentication, and ASP.NET Core 
has great support for both, relatively little custom code is required launch, authorize, and service 
LTI 1.3 requests.

[Microsoft.AspNetCore.App](https://www.nuget.org/packages/Microsoft.AspNetCore.App)
This package provides the framework for the web application, including user management. Each registered 
user gets their own test platform, with a test course, and two test people (one student and one teacher).    

[IdentityServer4.AspNetIdentity](https://www.nuget.org/packages/IdentityServer4.AspNetIdentity)
This package works with ASP.NET Core Identity to provide a robust OpenID Connect and OAuth 2.0 framework.
In particular, Identity Server provides the Authorize and Access Token endpoints that are used by LTI
Advantage Tools, and checks to make sure the tool has the right permissions to use the LTI Advantage
services (e.g. Names and Role Provisioning Services).

[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
This package provides some JSON Web Token functionality.

[LtiAdvantage](https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage)
This library makes implementing LTI Advantage with ASP.NET Core easier.

[LtiAdvantage.IdentityServer4](https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage.IdentityServer4)
This library adds an Identity Server 4 secret validator that understands the IMS recommended format of 
client-credentials grant.

[LtiAdvantage.IdentityModel](https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage.IdentityModel)
This library adds an HttpClient extension method to request a token using using the IMS recommended
format of client-credentials grant.

## Source Code Structure
Most of the code in this application is from the ASP.NET Core Web Application template for Razor Pages. This
is a map of where to find the LTI specific stuff.
```
/ (Visual Studio solution file)
└── src/
    ├── Startup.cs (configure ASP.NET Core Identity and Identity Server 4)
    ├── Areas/Identity/ (ASP.NET Core Identity user management, platform, course, and people editors)
    ├── Controllers/ (API controllers that implement Assignment and Grade Services and Names and Role Provisioning Services)
    ├── Data/ (database entities and Entity Framework Core migrations)
    ├── Pages/
        ├── DeepLinks (receives the deep links response)
	└── OidcLaunch (starts the LTI 1.3 launch process)
    ├── Utility/
        └── LtiAdvantageProfileService.cs (works with Identity Server 2 to add LTI claims to the id_token for launch)
    ├── wwwroot/ (static website files)        
```
