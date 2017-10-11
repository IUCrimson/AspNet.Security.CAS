using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public class CasOptions : RemoteAuthenticationOptions
    {
        public CasOptions()
        {
            CallbackPath = new PathString("/signin-cas");
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            TicketValidator = new Cas2TicketValidator();
            Events = new CasEvents();
        }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption { get; set; }
        
        /// <summary>
        /// The base url of the CAS server
        /// </summary>
        /// <example>https://cas.example.com/cas</example>
        public string CasServerUrlBase { get; set; }
        
        /// <summary>
        /// If set, and using the CAS 2 payload, the ticket validator use the specified CAS attribute as
        /// the NameIdentifier claim, which is used to associate external logins
        /// </summary>
        public string NameIdentifierAttribute { get; set; }

        /// <summary>
        /// Used by <see cref="CasHandler"/> to validate the CAS ticket and return an AuthenticationTicket with the user's CAS identity.
        /// </summary>
        public ICasTicketValidator TicketValidator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CasEvents"/> used to handle authentication events.
        /// </summary>
        public new CasEvents Events
        {
            get => (CasEvents) base.Events;
            set => base.Events = value;
        }
        
        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}