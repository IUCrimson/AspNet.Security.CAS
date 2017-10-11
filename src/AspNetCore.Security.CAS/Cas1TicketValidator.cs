using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Security.CAS
{
    public class Cas1TicketValidator : ICasTicketValidator
    {

        public async Task<AuthenticationTicket> ValidateTicket(HttpContext context, AuthenticationProperties properties, AuthenticationScheme scheme, CasOptions options, string ticket, string service)
        {
            var validateEndpoint = string.IsNullOrEmpty(options.CasValidationUrl) ? $"{options.CasServerUrlBase}/validate" : options.CasValidationUrl;
            var validateUrl = $"{validateEndpoint}?service={service}&ticket={Uri.EscapeDataString(ticket)}";

            var response = await options.Backchannel.GetAsync(validateUrl, context.RequestAborted);
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
                return null;
            }
            var issuer = options.ClaimsIssuer ?? scheme.Name;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, validatedUserName, ClaimValueTypes.String, issuer),
                new Claim(ClaimTypes.Name, validatedUserName, ClaimValueTypes.String, issuer)
            };

            var identity = new ClaimsIdentity(claims, options.ClaimsIssuer);
            var ticketContext = new CasCreatingTicketContext(context, scheme, options, new ClaimsPrincipal(identity), properties, validatedUserName);

            await options.Events.CreatingTicket(ticketContext);

            return new AuthenticationTicket(ticketContext.Principal, ticketContext.Properties, scheme.Name);
        }
    }
}