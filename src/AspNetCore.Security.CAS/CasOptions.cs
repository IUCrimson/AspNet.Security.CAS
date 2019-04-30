using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public class CasOptions : RemoteAuthenticationOptions
    {
        public CasOptions()
        {
            CallbackPath = new PathString("/signin-cas");
            TicketValidator = new Cas2TicketValidator();
            Events = new CasEvents();
        }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// The base url of the CAS server
        /// <para/>
        /// Used to redirect users to {CasServerUrlBase}/login when challenged.
        /// <para/>
        /// In the absence of <see cref="CasValidationUrl"/> being set, this same server base will
        /// validate tickets agains the {CasServerUrlBase}/serviceValidate endpoint.
        /// </summary>
        /// <example>https://cas.example.com/cas</example>
        public string CasServerUrlBase { get; set; }

        /// <summary>
        /// Used in cases where ticket validation occurs on a separate server than user login.
        /// <para/>
        /// Note: This is a full endpoint URL unlike <see cref="CasServerUrlBase"/> which is only a base URL.
        /// </summary>
        /// <example>https://cas.example.com/cas/serviceValidate</example>
        public string CasValidationUrl { get; set; }

        /// <summary>
        /// If set, and using the CAS 2 payload, the ticket validator uses the specified CAS attribute as
        /// the NameIdentifier claim, which is used to associate external logins
        /// </summary>
        public string NameIdentifierAttribute { get; set; }

        /// <summary>
        /// If set, and using the CAS 2 payload, the ticket validator uses the specified namespace when parsing the ticket
        /// </summary>
        public string TicketNamespace { get; set; }

        /// <summary>
        /// If set, and using the CAS 2 payload, the ticket validator uses the children of the specified CAS attribute as
        /// additional claims
        /// </summary>
        public string AttributesParent { get; set; }

        /// <summary>
        /// Used by <see cref="CasHandler"/> to validate the CAS ticket and return an AuthenticationTicket with the user's CAS identity.
        /// </summary>
        public ICasTicketValidator TicketValidator { get; set; }

        /// <summary>
        /// [OPTIONAL] - If this parameter is set, single sign-on will be bypassed. 
        /// In this case, CAS will require the client to present credentials regardless of the existence of a single sign-on session with CAS. 
        /// This parameter is not compatible with the `gateway` parameter. Services redirecting to the /login URI and login form views posting 
        /// to the /login URI SHOULD NOT set both the `renew` and `gateway` request parameters. Behavior is undefined if both are set. It is 
        /// RECOMMENDED that CAS implementations ignore the `gateway` parameter if `renew` is set. It is RECOMMENDED that when the renew 
        /// parameter is set itsvalue be `true`.
        /// </summary>
        public bool Renew { get; set; }

        /// <summary>
        /// [OPTIONAL] - If this parameter is set, CAS will not ask the client for credentials. If the client has a pre-existing single sign-on
        /// session with CAS, or if a single sign-on session can be established through non-interactive means (i.e. trust authentication), CAS
        /// MAY redirect the client to the URL specified by the `service` parameter, appending a valid service ticket. (CAS also MAY interpose 
        /// an advisory page informing the client that a CAS authentication has taken place.) If the client does not have a single sign-on session 
        /// with CAS, and a non-interactive authentication cannot be established, CAS MUST redirect the client to the URL specified by the `service` 
        /// parameter with no `ticket` parameter appended to the URL. If the `service` parameter is not specified and `gateway` is set, the behavior 
        /// of CAS is undefined. It is RECOMMENDED that in this case, CAS request credentials as if neither parameter was specified. This parameter 
        /// is not compatible with the `renew` parameter. Behavior is undefined if both are set. It is RECOMMENDED that when the gateway parameter 
        /// is set its value be `true`.
        /// </summary>
        public bool Gateway { get; set; }

        /// <summary>
        /// Specify the domain name used in the `service` parameter.
        /// <para />
        /// The default behavior to build the `service` parameter uses `Request.Host`, however, in certain containerized hosting scenarios this
        /// value may resolve to the container name instead of the public facing domain name.
        /// <para />
        /// See: https://github.com/IUCrimson/AspNet.Security.CAS/issues/16
        /// </summary>
        /// <remarks>Trailing slashes will be stripped.</remarks>
        /// <example>www.mywebsite.com</example>
        public string ServiceHost { get; set; }

        /// <summary>
        /// Force the URI in the `service` parameter to use HTTPS.
        /// <para />
        /// The default behavior to build the `service` parameter uses `Request.Scheme`, however, in certain reverse-proxied environments this
        /// value may not reflect the original public address of the URI.
        /// <para />
        /// See: https://github.com/IUCrimson/AspNet.Security.CAS/issues/17
        /// </summary>
        /// <example>true</example>
        public bool ServiceForceHTTPS { get; set; } = false;

        /// <summary>
        /// Escape the the service parameter string when sending requests to the CAS server
        /// <para />
        /// The default behavior to build the `service uri` is to escape, however, not all CAS
        /// server implementations will decode this value on receiving it.
        /// </summary>
        /// <example>false</example>
        public bool EscapeServiceString { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="CasEvents"/> used to handle authentication events.
        /// </summary>
        public new CasEvents Events
        {
            get => (CasEvents)base.Events;
            set => base.Events = value;
        }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}