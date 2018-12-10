# LTI Advantage Platform

Sample LTI Advantage Platform using .NET Core 2.1. So that it has a purpose (other than as a reference code sample), you can use it to [test](https://advantageplatform.azurewebsites.net/) LTI 1.3 Tools. There is a sample LTI Advantage Tool project over [here](https://github.com/andyfmiller/LtiAdvantageTool).

## Supported Features

- LTI 1.3 Core (launch)
   - Register yourself on the Platform to access the Tools UI and create a Client ID for your Tool (this is similar to Google's Developer Console). Then create a Resource Link and launch it.
- Assignment and Grade Services v2
   - Tools can get all line items for a course, get a specific line item, and get results from the platform; and they can post scores to the platform.
- Names and Role Provisioning Services v2
   - Tools can get course membership from the platform.

## Dependencies

This web application is based on the current .NET Core Web Application template for Razor Pages.
Most of the application is dedicated to managing Tools. Because LTI Advantage uses OpenID Connect
and OAuth to handle authorization and authentication, and ASP.NET Core has great support for both,
relatively little code is required launch, authorize, and service LTI requests.

I The packages and projects below take care of all the core OpenId Connect and OAuth stuff:

<dl>
  <dt>Microsoft.AspNetCore.App (https://www.nuget.org/packages/Microsoft.AspNetCore.App)</dt>
  <dd>This package provides the framework for the web application, including user management (application users, not LTI users).</dd>
  <dt>IdentityServer4.AspNetIdentity (https://www.nuget.org/packages/IdentityServer4.AspNetIdentity)</dt>
  <dd>This package works with ASP.NET Core Identity to provide a robust OpenID Connect and OAuth 2.0 framework. For example, IS provides the Authorize and Access Token endpoints that are used by LTI Advantage Tools.</dd>
  <dt>System.IdentityModel.Tokens.Jwt (https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)</dt>
  <dd>This package provides some JSON Web Token functionality.</dd>
  <dt>LtiAdvantage (https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage)</dt>
  <dd>This library makes implementing LTI Advantage easier with ASP.NET Core.</dd>
  <dt>LtiAdvantage.IdentityServer4 (https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage.IdentityServer4)</dt>
  <dd>This library adds an Identity Server 4 secret validator that understands the IMS recommended format of client-credentials grant.</dd>
  <dt>LtiAdvantage.IdentityModel (https://github.com/andyfmiller/LtiAdvantage/tree/master/src/LtiAdvantage.IdentityModel)</dt>
  <dd>This library adds an HttpClient extension method to request a token using using the IMS recommended format of client-credentials grant.</dd>
</dl>
