using Microsoft.Extensions.DependencyInjection;
using MauiSsoLibrary.Services;
using System;

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

            // Register platform-specific token store
#if ANDROID
            services.AddSingleton<ITokenStore>(sp =>
            {
                var context = Android.App.Application.Context;
                if (context == null)
                    throw new InvalidOperationException("Android Application Context is null");

                System.Diagnostics.Debug.WriteLine("SsoServiceExtensions: Registering SharedTokenStore");
                return new SharedTokenStore(context);
            });
#else
            services.AddSingleton<ITokenStore, TokenStore>();
#endif

            services.AddSingleton<IOidcAuthService, OidcAuthService>();

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

            // Register platform-specific token store
#if ANDROID
            services.AddSingleton<ITokenStore>(sp =>
            {
                var context = Android.App.Application.Context;
                if (context == null)
                    throw new InvalidOperationException("Android Application Context is null");

                System.Diagnostics.Debug.WriteLine("SsoServiceExtensions: Registering SharedTokenStore");
                return new SharedTokenStore(context);
            });
#else
            services.AddSingleton<ITokenStore, TokenStore>();
#endif

            services.AddSingleton<IOidcAuthService, OidcAuthService>();

            return services;
        }
    }
}