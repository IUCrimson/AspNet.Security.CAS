AspNetCore.Security.CAS
===================

AspNet.Security.CAS is an [ASP.NET Core 2/MVC 6](https://docs.microsoft.com/en-us/aspnet/core/) authentication provider for [CAS](https://github.com/apereo/cascas).

This implimentation is based upon on Noel Bundick's [Owin.Security.CAS](https://github.com/noelbundick/Owin.Security.CAS) provider.

[Microsoft.AspNetCore.Authentication.OAuth](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNetCore.Authentication.Twitter) and [Microsoft.AspNetCore.Authentication.Twitter](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNetCore.Authentication.Twitter) were used as structural references.

## Usage

1. Install the NuGet package

    `PM> Install-Package AspNet.Security.CAS`

1. Open **Startup.cs**

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

1. In a controller somewhere, create a login endpoint.  It doesn't have to auto-challenge/redirect but this example does:

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

These options extend the [RemoteAuthenticationOptions](https://github.com/aspnet/Security/blob/dev/src/Microsoft.AspNetCore.Authentication/RemoteAuthenticationOptions.cs) class.

**Properties**

* `CasServerUrlBase` - The base url of the CAS server.  Required.

* `CasValidationUrl` - Used in cases where ticket validation occurs on a separate server than user login.  Default: `null`

* `TicketValidator` - Gets or sets the `ICasTicketValidator` used to validate tickets from CAS. Default: `Cas2TicketValidator`

* `Caption` - Get or sets the text that the user can display on a sign in user interface.  Default: "CAS".

* `SignInAsAuthenticationType` - Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user `ClaimsIdentity`.

* `StateDataFormat` - Gets or sets the type used to secure data handled by the middleware.

* `NameClaimType` - If set, and using the CAS 2 payload, the ticket validator will set the NameClaimType to the specified CAS attribute rather than using the default Name claim.

* `NameIdentifierAttribute` - If set, and using the CAS 2 payload, the ticket validator use the specified CAS attribute as the NameIdentifier claim, which is used to associate external logins.

**Inherited Properties**

* `BackchannelTimeout` - Gets or sets timeout value in milliseconds for back channel communications with CAS.  Default: 60s

* `HttpMessageHandler` - The HttpMessageHandler used to communicate with CAS.  

* `CallbackPath` - The request path within the application's base path where the user-agent will be returned.  The middleware will process this request when it arrives.

* `SignInScheme` - Gets or sets the authentication scheme corresponding to the middleware responsible of persisting user's identity after a successful authentication.  This value typically corresponds to a cookie middleware registered in the Startup class.  When omitted, `SharedAuthenticationOptions.SignInScheme` is used as a fallback value.

## Other CAS Providers

MVC 5: [Owin.Security.CAS](https://github.com/noelbundick/Owin.Security.CAS)

MVC 4 and below: [Jasig's .NET CAS Client](https://github.com/Jasig/dotnet-cas-client)


[![MIT license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/IUCrimson/AspNet.Security.CAS/blob/master/LICENSE.md)
