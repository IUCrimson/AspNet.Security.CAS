using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace AspNetCore.Security.CAS
{
    public class CasRedirectToAuthorizationEndpointContext : BaseCasContext
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <param name="options">The Cas middleware options.</param>
        /// <param name="properties">The authentication properties of the challenge.</param>
        /// <param name="redirectUri">The initial redirect URI.</param>
        public CasRedirectToAuthorizationEndpointContext(HttpContext context, CasOptions options,
            AuthenticationProperties properties, string redirectUri)
            : base(context, options)
        {
            RedirectUri = redirectUri;
            Properties = properties;
        }

        /// <summary>
        /// Gets the URI used for the redirect operation.
        /// </summary>
        public string RedirectUri { get; private set; }

        /// <summary>
        /// Gets the authentication properties of the challenge.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}