AspNet.Security.CAS
===================

AspNet.Security.CAS is an [ASP.NET 5/MVC 6](http://docs.asp.net/en/1.0.0-rc1/) authentication provider for [CAS](https://github.com/Jasig/cas).

This implimentation is based upon on Noel Bundick's [Owin.Security.CAS](https://github.com/noelbundick/Owin.Security.CAS) provider.

[Microsoft.AspNet.Authentication.OAuth](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNet.Authentication.Twitter) and [Microsoft.AspNet.Authentication.Twitter](https://github.com/aspnet/Security/tree/dev/src/Microsoft.AspNet.Authentication.Twitter) were used as structural references.

## Usage

1. Install the NuGet package

    `PM> Install-Package AspNet.Security.CAS`

1. Open **Startup.cs**

1. Paste the following code below `// Uncomment the following lines to enable logging in with third party login providers`


    ```c#
    app.UseCasAuthentication(options =>
    {
        options.CasServerUrlBase = "https://your.cas.server.com/cas";
    });
    ```

## CasOptions

At a minmum, the `CasOptions` object needs to have the `CasServerUrlBase` property set to the URL to your CAS server.

These options extend the [RemoteAuthenticationOptions](https://github.com/aspnet/Security/blob/dev/src/Microsoft.AspNet.Authentication/RemoteAuthenticationOptions.cs) class.

**Properties**

* `CasServerUrlBase` - The base url of the CAS server.  Required.

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