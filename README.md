AspNetCore.Security.CAS
===================

A [CAS 2.0](https://apereo.github.io/cas/5.2.x/protocol/CAS-Protocol-V2-Specification.html) authentication provider for [ASP.NET Core 2](https://docs.microsoft.com/en-us/aspnet/core/),
based on Microsoft's providers for [OAuth](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNetCore.Authentication.OAuth) and [Twitter](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNetCore.Authentication.Twitter).

## Usage

1. Install the NuGet package

    `PM> Install-Package AspNetCore.Security.CAS`

1. Open `Startup.cs`

1. In your startup's `ConfigureServices` method:

	```c#
	services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
		.AddCookie(options =>
		{
			options.LoginPath = new PathString("/login");
		})
		.AddCAS(options =>
		{
			options.CasServerUrlBase = Configuration["CasBaseUrl"];   // Set in `appsettings.json` file.
			options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		});
	```

1. In your startup's `Configure` method before `UseMvc`:

    ```c#
    app.UseAuthentication();
    ```

1. Create a login endpoint.  It doesn't have to automatically challenge but this example does:

	```c#
    [AllowAnonymous]
    [Route("login")]
    public async Task Login(string returnUrl)
    {
        var props = new AuthenticationProperties { RedirectUri = returnUrl };
        await HttpContext.ChallengeAsync("CAS", props);
    }
	```

## CasOptions

At a minmum, the `CasOptions` object needs to have the `CasServerUrlBase` property set to the URL to your CAS server.

These options extend the [RemoteAuthenticationOptions](https://github.com/aspnet/Security/blob/rel/2.0.0/src/Microsoft.AspNetCore.Authentication/RemoteAuthenticationOptions.cs) class.

**Properties**

| Property                  | Description                                                                                                                                                                                                                                                                                                                                                          | Default                                                                                                                                            |
|---------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| `CasServerUrlBase`        | The base url of the CAS server. Required.                                                                                                                                                                                                                                                                                                                            | `null`                                                                                                                                             |
| `CasValidationUrl`        | Used in cases where ticket validation occurs on a separate server than user login.  Optional.                                                                                                                                                                                                                                                                        | `null`                                                                                                                                             |
| `TicketValidator`         | Gets or sets the `ICasTicketValidator` used to validate tickets from CAS.                                                                                                                                                                                                                                                                                            | `Cas2TicketValidator`                                                                                                                              |
| `StateDataFormat`         | Gets or sets the type used to secure data handled by the middleware.                                                                                                                                                                                                                                                                                                 | [`PropertiesDataFormat`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.propertiesdataformat?view=aspnetcore-2.0) |
| `NameIdentifierAttribute` | If set, and using the CAS 2 payload, the ticket validator use the specified CAS attribute as the NameIdentifier claim, which is used to associate external logins.                                                                                                                                                                                                   | `null`                                                                                                                                             |
| `Renew`                   | If this parameter is set, single sign-on will be bypassed. In this case, CAS will require the client to present credentials regardless of the existence of a single sign-on session with CAS.                                                                                                                                                                        | `false`                                                                                                                                            |
| `Gateway`                 | If this parameter is set, CAS will not ask the client for credentials. If the client has a pre-existing single sign-on session with CAS, or if a single sign-on session can be established through non-interactive means (i.e. trust authentication), CAS MAY redirect the client to the URL specified by the `service` parameter, appending a valid service ticket. | `false`                                                                                                                                            |
| `ServiceHost`             | Specify the domain name used in the `service` parameter.                                                                                                                                                                                                                                                                                                             | `null`                                                                                                                                             |
| `ServiceForceHTTPS`       | Specify that exchanges with the CAS endpoint use `https`                                                                                                                                                                                                                                                                                                             | `false`                                                                                                                                            |
| `EscapeServiceString`     | Escape the the service parameter string when sending requests to the CAS server. The default behavior to build the `service uri` is to escape, however, not all CAS server implementations will decode this value on receiving it.																													               | `true`                                                                                                                                             |

*See the [documentation for optional properties](https://apereo.github.io/cas/5.0.x/protocol/CAS-Protocol-V2-Specification.html#211-parameters) for more information if using `Renew` or `Gateway`.*

## Other .NET CAS Providers

MVC 5: [Owin.Security.CAS](https://github.com/noelbundick/Owin.Security.CAS)

MVC 4 and below: [Jasig's .NET CAS Client](https://github.com/Jasig/dotnet-cas-client)


[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/IUCrimson/AspNet.Security.CAS/blob/master/LICENSE.md)
