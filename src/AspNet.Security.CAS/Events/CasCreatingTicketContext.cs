using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace AspNet.Security.CAS
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class CasCreatingTicketContext: BaseCasContext
    {
        /// <summary>
        /// Initializes a <see cref="CasCreatingTicketContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="options"></param>
        /// <param name="userName">Cas user ID</param>
        public CasCreatingTicketContext(
            HttpContext context,
            CasOptions options,
            string userName)
            : base(context, options)
        {
            Username = userName;
        }

        /// <summary>
        /// Gets the Cas user ID
        /// </summary>
        public string Username { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}
