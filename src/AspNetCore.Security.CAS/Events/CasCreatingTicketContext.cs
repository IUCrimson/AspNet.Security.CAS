using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AspNetCore.Security.CAS
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class CasCreatingTicketContext : ResultContext<CasOptions>
    {
        /// <summary>
        /// Initializes a <see cref="CasCreatingTicketContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="scheme"></param>
        /// <param name="options"></param>
        /// <param name="principal"></param>
        /// <param name="properties"></param>
        /// <param name="userName">Cas user ID</param>
        public CasCreatingTicketContext(
            HttpContext context,
            AuthenticationScheme scheme,
            CasOptions options,
            ClaimsPrincipal principal,
            AuthenticationProperties properties,
            string userName)
            : base(context, scheme, options)
        {
            Username = userName;
            Principal = principal;
            Properties = properties;
        }

        /// <summary>
        /// Gets the Cas user ID
        /// </summary>
        public string Username { get; }
    }
}