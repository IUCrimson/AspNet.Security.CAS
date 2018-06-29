using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Security.CAS
{
    internal class CasHandler : RemoteAuthenticationHandler<CasOptions>
    {
        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring.
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected new CasEvents Events
        {
            get => (CasEvents)base.Events;
            set => base.Events = value;
        }

        public CasHandler(IOptionsMonitor<CasOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<object> CreateEventsAsync() => Task.FromResult<object>(new CasEvents());

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            AuthenticationProperties properties = null;
            var query = Request.Query;
            var state = query["state"];

            properties = Options.StateDataFormat.Unprotect(state);
            if (properties == null)
            {
                return HandleRequestResult.Fail("The state was missing or invalid.");
            }

            // OAuth2 10.12 CSRF
            if (!ValidateCorrelationId(properties))
            {
                return HandleRequestResult.Fail("Correlation failed.");
            }

            var casTicket = query["ticket"];
            if (string.IsNullOrEmpty(casTicket))
            {
                return HandleRequestResult.Fail("Missing CAS ticket.");
            }

            var authTicket = await Options.TicketValidator.ValidateTicket(Context, properties, Scheme, Options, casTicket, BuildReturnTo(state));
            if (authTicket == null)
            {
                return HandleRequestResult.Fail("Failed to retrieve user information from remote server.");
            }

            return HandleRequestResult.Success(authTicket);
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = CurrentUri;
            }

            // OAuth2 10.12 CSRF
            GenerateCorrelationId(properties);

            var returnTo = BuildReturnTo(Options.StateDataFormat.Protect(properties));

            var authorizationEndpoint = $"{Options.CasServerUrlBase}/login?service={returnTo}";

            if (Options.Renew)
            {
                authorizationEndpoint += "&renew=true";
            }

            if (Options.Gateway)
            {
                authorizationEndpoint += "&gateway=true";
            }

            var redirectContext = new RedirectContext<CasOptions>(Context, Scheme, Options, properties, authorizationEndpoint);

            await Options.Events.RedirectToAuthorizationEndpoint(redirectContext);
        }

        private string BuildReturnTo(string state)
        {
            var host = Request.Host;
            var scheme = Request.Scheme;

            if (!string.IsNullOrWhiteSpace(Options.ServiceHost))
            {
                host = new HostString(Options.ServiceHost.Replace("/", ""));
            }

            if (Options.ServiceForceHTTPS)
            {
                scheme = "https";
            }

            var returnTo = $"{scheme}://{host}{Request.PathBase}{Options.CallbackPath}?state={Uri.EscapeDataString(state)}";

            return Options.EscapeServiceString ? Uri.EscapeDataString(returnTo) : returnTo;
        }
    }
}