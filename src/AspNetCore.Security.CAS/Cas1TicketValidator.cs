using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Security.CAS
{
    public class Cas1TicketValidator : ICasTicketValidator
    {
        private readonly CasOptions _options;

        public Cas1TicketValidator(CasOptions options)
        {
            _options = options;
        }

        public async Task<AuthenticateResult> ValidateTicket(HttpContext context, HttpClient httpClient, AuthenticationProperties properties, string ticket, string service)
        {
            var validateUrl = _options.CasServerUrlBase + "/validate" +
                              "?service=" + service +
                              "&ticket=" + Uri.EscapeDataString(ticket);

            var response = await httpClient.GetAsync(validateUrl, context.RequestAborted);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            string validatedUserName = null;
            var responseParts = responseBody.Split('\n');
            if (responseParts.Length >= 2 && responseParts[0] == "yes")
            {
                validatedUserName = responseParts[1];
            }

            if (string.IsNullOrEmpty(validatedUserName))
            {
                return AuthenticateResult.Fail("Could find username in CAS response.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, validatedUserName, ClaimValueTypes.String, _options.ClaimsIssuer),
                new Claim(ClaimTypes.Name, validatedUserName, ClaimValueTypes.String, _options.ClaimsIssuer)
            };

            var identity = new ClaimsIdentity(claims, _options.ClaimsIssuer);

            var ticketContext = new CasCreatingTicketContext(context, _options, identity.Name)
            {
                Principal = new ClaimsPrincipal(identity),
                Properties = properties
            };

            await _options.Events.CreatingTicket(ticketContext);

            return AuthenticateResult.Success(new AuthenticationTicket(ticketContext.Principal, ticketContext.Properties, _options.AuthenticationScheme));
        }
    }
}