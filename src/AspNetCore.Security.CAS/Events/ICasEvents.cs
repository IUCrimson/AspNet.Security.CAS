using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

namespace AspNetCore.Security.CAS
{
    /// <summary>
    /// Specifies callback methods which the <see cref="CasMiddleware"></see> invokes to enable developer control over the authentication process. />
    /// </summary>
    public interface ICasEvents : IRemoteAuthenticationEvents
    {
        /// <summary>
        /// Invoked whenever Cas succesfully authenticates a user.
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        Task CreatingTicket(CasCreatingTicketContext context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Cas middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="Http.Authentication.AuthenticationProperties" /> </param>
        Task RedirectToAuthorizationEndpoint(CasRedirectToAuthorizationEndpointContext context);
    }
}