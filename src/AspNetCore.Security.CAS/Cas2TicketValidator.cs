using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AspNetCore.Security.CAS
{
    public class Cas2TicketValidator : ICasTicketValidator
    {
        private readonly XNamespace _ns = "http://www.yale.edu/tp/cas";

        public async Task<AuthenticationTicket> ValidateTicket(HttpContext context, AuthenticationProperties properties, AuthenticationScheme scheme, CasOptions options, string ticket, string service)
        {
            var validateUrl = options.CasServerUrlBase + "/serviceValidate" +
                              "?service=" + service +
                              "&ticket=" + Uri.EscapeDataString(ticket);

            var response = await options.Backchannel.GetAsync(validateUrl, context.RequestAborted);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(responseBody);

            var serviceResponse = doc.Element(_ns + "serviceResponse");
            var successNode = serviceResponse?.Element(_ns + "authenticationSuccess");
            var userNode = successNode?.Element(_ns + "user");
            var validatedUserName = userNode?.Value;

            if (string.IsNullOrEmpty(validatedUserName))
            {
                return null;
            }

            var identity = BuildIdentity(options, scheme, validatedUserName, successNode);
            var ticketContext = new CasCreatingTicketContext(context, scheme, options, new ClaimsPrincipal(identity), properties, validatedUserName);

            await options.Events.CreatingTicket(ticketContext);

            return new AuthenticationTicket(ticketContext.Principal, ticketContext.Properties, scheme.Name);
        }

        private ClaimsIdentity BuildIdentity(CasOptions options, AuthenticationScheme scheme, string username, XContainer successNode)
        {
            var issuer = options.ClaimsIssuer ?? scheme.Name;

            var identity = new ClaimsIdentity(issuer);
            identity.AddClaim(new Claim(ClaimTypes.Name, username, ClaimValueTypes.String, issuer));

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
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identityValue, ClaimValueTypes.String, issuer));

            return identity;
        }
    }
}