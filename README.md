# LTI Advantage Platform

Sample LTI Advantage Platform using .NET Core 2.1. So that it has a purpose (other than as a reference code sample), you can use it to [test](https://advantageplatform.azurewebsites.net/) LTI 1.3 Tools. There is a sample LTI Advantage Tool project over [here](https://github.com/andyfmiller/LtiAdvantageTool).

## Supported Features

- LTI 1.3 Core (launch)
- The rest of LTI Advantage is coming soon

## Dependencies

This web application is based on the current .NET Core Web Application template for Razor Pages.
Almost all the custom code is manage the data this test harness uses. The packages and projects
below take care of all the actual Json Web Token (JWT) creation, signing, and validating.

<dl>
  <dt>Microsoft.AspNetCore.App (https://www.nuget.org/packages/Microsoft.AspNetCore.App)</dt>
  <dd>This provides the framework for the web application.</dd>
  <dt>IdentityServer4.AspNetIdentity (https://www.nuget.org/packages/IdentityServer4.AspNetIdentity)</dt>
  <dd>This package manages the private and public keys, signing the JWT, provides a well known discovery API, and
    a KeySet interface.</dd>
  <dt>System.IdentityModel.Tokens.Jwt (https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)</dt>
  <dd>This package provides some JWT functionality, but may be removed in favor of IdentityServer.</dd>
  <dt>LtiAdvantageLibrary (https://github.com/andyfmiller/LtiAdvantageLibrary)</dt>
  <dd>This library provides some LTI specific features.</dd>
</dl>
