using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AspNetCore.Security.CAS
{
    public class Cas2TicketValidator : ICasTicketValidator
    {
        private readonly CasOptions _options;
        private readonly XNamespace _ns = "http://www.yale.edu/tp/cas";

        public Cas2TicketValidator(CasOptions options)
        {
            _options = options;
        }

        public async Task<AuthenticateResult> ValidateTicket(HttpContext context, HttpClient httpClient, AuthenticationProperties properties, string ticket, string service)
        {
            var validateUrl = _options.CasServerUrlBase + "/serviceValidate" +
                              "?service=" + service +
                              "&ticket=" + Uri.EscapeDataString(ticket);

            var response = await httpClient.GetAsync(validateUrl, context.RequestAborted);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(responseBody);

            var serviceResponse = doc.Element(_ns + "serviceResponse");
            var successNode = serviceResponse?.Element(_ns + "authenticationSuccess");
            var userNode = successNode?.Element(_ns + "user");
            var validatedUserName = userNode?.Value;

            if (string.IsNullOrEmpty(validatedUserName))
            {
                return AuthenticateResult.Fail("Could find username in CAS response.");
            }

            var identity = BuildIdentity(_options, validatedUserName, successNode);

            var ticketContext = new CasCreatingTicketContext(context, _options, identity.Name)
            {
                Principal = new ClaimsPrincipal(identity),
                Properties = properties
            };

            await _options.Events.CreatingTicket(ticketContext);

            return ticketContext.Principal?.Identity == null
                ? AuthenticateResult.Fail("There was a problem creating ticket.")
                : AuthenticateResult.Success(new AuthenticationTicket(ticketContext.Principal, ticketContext.Properties, _options.AuthenticationScheme));
        }

        private ClaimsIdentity BuildIdentity(CasOptions options, string username, XContainer successNode)
        {
            var identity = new ClaimsIdentity(options.ClaimsIssuer, options.NameClaimType, ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, options.ClaimsIssuer));

            var attributesNode = successNode.Element(_ns + "attributes");
            if (attributesNode != null)
            {
                foreach (var element in attributesNode.Elements())
                {
                    identity.AddClaim(new Claim(element.Name.LocalName, element.Value));
                }
            }

            var identityValue = username;
            if (options.NameIdentifierAttribute != null && attributesNode != null)
            {
                var identityAttribute = attributesNode.Elements().FirstOrDefault(x => x.Name.LocalName == options.NameIdentifierAttribute);
                if (identityAttribute == null)
                {
                    throw new Exception($"Identity attribute [{options.NameIdentifierAttribute}] not found for user: {username}");
                }

                identityValue = identityAttribute.Value;
            }
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identityValue, ClaimValueTypes.String, options.ClaimsIssuer));

            return identity;
        }
    }
}