using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;

namespace AspNet.Security.CAS
{
    internal class CasHandler : RemoteAuthenticationHandler<CasOptions>
    {
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        private const string StateCookie = "__CasState";
        private const string CorrelationPrefix = ".AspNet.Correlation.";
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
                return AuthenticateResult.Failed("The state was missing or invalid.");
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
                return AuthenticateResult.Failed("Correlation failed.");
            }

            var ticket = query["ticket"];
            if (string.IsNullOrEmpty(ticket))
            {
                return AuthenticateResult.Failed("Missing CAS ticket.");
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

        protected void GenerateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var correlationKey = CorrelationPrefix + Options.AuthenticationScheme;

            var nonceBytes = new byte[32];
            CryptoRandom.GetBytes(nonceBytes);
            var correlationId = Base64UrlTextEncoder.Encode(nonceBytes);

            properties.Items[correlationKey] = correlationId;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            Response.Cookies.Append(correlationKey, correlationId, cookieOptions);
        }
        
        protected bool ValidateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var correlationKey = CorrelationPrefix + Options.AuthenticationScheme;
            var correlationCookie = Request.Cookies[correlationKey];
            if (string.IsNullOrEmpty(correlationCookie))
            {
                return false;
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            Response.Cookies.Delete(correlationKey, cookieOptions);

            string correlationExtra;
            if (!properties.Items.TryGetValue(correlationKey, out correlationExtra))
            {
                return false;
            }

            properties.Items.Remove(correlationKey);

            return string.Equals(correlationCookie, correlationExtra, StringComparison.Ordinal);
        }
    }
}
