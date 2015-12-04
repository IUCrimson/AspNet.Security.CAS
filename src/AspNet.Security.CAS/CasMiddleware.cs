using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.WebEncoders;

namespace AspNet.Security.CAS
{
    public class CasMiddleware : AuthenticationMiddleware<CasOptions>
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a <see cref="CasMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the HTTP pipeline to invoke</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="encoder"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="options">Configuration options for the middleware</param>
        /// <param name="configureOptions"></param>
        public CasMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IOptions<SharedAuthenticationOptions> sharedOptions,
            CasOptions options)
            : base(next, options, loggerFactory, encoder)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (sharedOptions == null)
            {
                throw new ArgumentNullException(nameof(sharedOptions));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }


            if (!Options.CallbackPath.HasValue)
            {
                //throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Microsoft.AspNet.Authentication.Resources.Exception_OptionMustBeProvided, nameof(Options.CallbackPath)));
                throw new ArgumentException(string.Format("CallbackPath must have a value.", nameof(Options.CallbackPath)));
            }

            if (string.IsNullOrEmpty(Options.CasServerUrlBase))
            {
                throw new ArgumentNullException("CasServerUrlBase in options was not set - it must point to the CAS server URL");
            }


            if (Options.Events == null)
            {
                Options.Events = new CasEvents();
            }

            if (Options.StateDataFormat == null)
            {
                var dataProtector = dataProtectionProvider.CreateProtector(
                    typeof(CasMiddleware).FullName, Options.AuthenticationScheme, "v1");
                Options.StateDataFormat = new PropertiesDataFormat(dataProtector);

            }

            if (string.IsNullOrEmpty(Options.SignInScheme))
            {
                Options.SignInScheme = sharedOptions.Value.SignInScheme;
            }

            _httpClient = new HttpClient(Options.BackchannelHttpHandler ?? new HttpClientHandler());
            _httpClient.Timeout = Options.BackchannelTimeout;
            _httpClient.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ASP.NET CAS middleware");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="CasOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<CasOptions> CreateHandler()
        {
            return new CasHandler(_httpClient);
        }
    }
}
