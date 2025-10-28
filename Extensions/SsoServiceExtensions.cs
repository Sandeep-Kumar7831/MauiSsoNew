using Microsoft.Extensions.DependencyInjection;
using MauiSsoLibrary.Services;

namespace MauiSsoLibrary.Extensions
{
    public static class SsoServiceExtensions
    {
        /// <summary>
        /// Add SSO services to the DI container
        /// </summary>
        public static IServiceCollection AddMauiSso(
            this IServiceCollection services,
            Action<SsoConfiguration> configureOptions)
        {
            var config = new SsoConfiguration();
            configureOptions(config);

            if (!config.IsValid())
                throw new ArgumentException("Invalid SSO configuration. Authority, ClientId, and RedirectUri are required.");

            services.AddSingleton(config);
            services.AddSingleton<ITokenStore, TokenStore>();
          //  services.AddSingleton<IOidcAuthService, OidcAuthService>();

            return services;
        }

        /// <summary>
        /// Add SSO services with configuration instance
        /// </summary>
        public static IServiceCollection AddMauiSso(
            this IServiceCollection services,
            SsoConfiguration configuration)
        {
            if (!configuration.IsValid())
                throw new ArgumentException("Invalid SSO configuration. Authority, ClientId, and RedirectUri are required.");

            services.AddSingleton(configuration);
            services.AddSingleton<ITokenStore, TokenStore>();
           // services.AddSingleton<IOidcAuthService, OidcAuthService>();

            return services;
        }
    }
}

