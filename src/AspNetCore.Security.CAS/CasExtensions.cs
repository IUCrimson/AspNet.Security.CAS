using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AspNetCore.Security.CAS
{
    public static class CasExtensions
    {
        public static AuthenticationBuilder AddCAS(this AuthenticationBuilder builder)
            => builder.AddCAS(CasDefaults.AuthenticationScheme, _ => { });

        public static AuthenticationBuilder AddCAS(this AuthenticationBuilder builder, Action<CasOptions> configureOptions)
            => builder.AddCAS(CasDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddCAS(this AuthenticationBuilder builder, string authenticationScheme, Action<CasOptions> configureOptions)
            => builder.AddCAS(authenticationScheme, CasDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddCAS(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<CasOptions> configureOptions)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CasOptions>, CasPostConfigureOptions>());
            return builder.AddRemoteScheme<CasOptions, CasHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}
