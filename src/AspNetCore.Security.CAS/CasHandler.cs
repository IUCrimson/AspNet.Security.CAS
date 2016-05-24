using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Security.CAS
{
    internal class CasHandler : RemoteAuthenticationHandler<CasOptions>
    {
        private const string StateCookie = "__CasState";
        private readonly HttpClient _httpClient;

        public CasHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected override async Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var properties = new AuthenticationProperties(context.Properties);
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }

            // OAuth2 10.12 CSRF
            GenerateCorrelationId(properties);

            var returnTo = BuildReturnTo(Options.StateDataFormat.Protect(properties));

            var authorizationEndpoint = $"{Options.CasServerUrlBase}/login?service={Uri.EscapeDataString(returnTo)}";

            var redirectContext = new CasRedirectToAuthorizationEndpointContext(
                Context, Options,
                properties, authorizationEndpoint);
            await Options.Events.RedirectToAuthorizationEndpoint(redirectContext);
            return true;
        }

        protected override async Task<AuthenticateResult> HandleRemoteAuthenticateAsync()
        {
            var query = Request.Query;
            var state = query["state"];

            var properties = Options.StateDataFormat.Unprotect(state);
            if (properties == null)
            {
                return AuthenticateResult.Fail("The state was missing or invalid.");
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            Response.Cookies.Delete(StateCookie, cookieOptions);

            // OAuth2 10.12 CSRF
            if (!ValidateCorrelationId(properties))
            {
                return AuthenticateResult.Fail("Correlation failed.");
            }

            var ticket = query["ticket"];
            if (string.IsNullOrEmpty(ticket))
            {
                return AuthenticateResult.Fail("Missing CAS ticket.");
            }

            var service = Uri.EscapeDataString(BuildReturnTo(state));

            return await Options.TicketValidator.ValidateTicket(Context, _httpClient, properties, ticket, service);
        }

        private string BuildReturnTo(string state)
        {
            return Request.Scheme + "://" + Request.Host +
                   Request.PathBase + Options.CallbackPath +
                   "?state=" + Uri.EscapeDataString(state);
        }
    }
}