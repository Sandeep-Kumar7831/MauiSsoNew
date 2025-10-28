using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using System;

namespace MauiSsoLibrary.Services
{
    public class OidcAuthService : IOidcAuthService
    {
        private readonly OidcClient _oidcClient;
        private readonly ITokenStore _tokenStore;
        private readonly SsoConfiguration _config;

        public OidcAuthService(ITokenStore tokenStore, SsoConfiguration config)
        {
            _tokenStore = tokenStore;
            _config = config;

            if (!config.IsValid())
                throw new ArgumentException("Invalid SSO configuration");

            var options = new OidcClientOptions
            {
                Authority = config.Authority,
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                Scope = config.Scope,
                RedirectUri = config.RedirectUri,
                PostLogoutRedirectUri = config.PostLogoutRedirectUri,
                Browser = new WebAuthenticatorBrowser()
            };

            _oidcClient = new OidcClient(options);
        }

        public async Task<AuthResult> LoginAsync()
        {
            try
            {
                var result = await _oidcClient.LoginAsync(new LoginRequest());

                if (result.IsError)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = result.Error,
                        ErrorDescription = result.ErrorDescription
                    };
                }

                var tokenResponse = new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    IdToken = result.IdentityToken

                };

                await _tokenStore.SaveTokensAsync(tokenResponse);

#if ANDROID
                if (_config.EnableAndroidService)
                {
                    StartAndroidService();
                }
#endif

                return new AuthResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Error = "Exception",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task<AuthResult> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = _tokenStore.GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = "NoRefreshToken"
                    };
                }

                var result = await _oidcClient.RefreshTokenAsync(refreshToken);

                if (result.IsError)
                {
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = result.Error,
                        ErrorDescription = result.ErrorDescription
                    };
                }

                var tokenResponse = new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    IdToken = result.IdentityToken,
                    ExpiresAt = result.AccessTokenExpiration
                };

                await _tokenStore.SaveTokensAsync(tokenResponse);
                return new AuthResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    Error = "Exception",
                    ErrorDescription = ex.Message
                };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var idToken = _tokenStore.GetIdToken();
                if (!string.IsNullOrEmpty(idToken))
                {
                    await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = idToken });
                }

#if ANDROID
                if (_config.EnableAndroidService)
                {
                    StopAndroidService();
                }
#endif

                _tokenStore.ClearTokens();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout error: {ex.Message}");
            }
        }

        public bool IsAuthenticated()
        {
            return _tokenStore.IsAuthenticated();
        }

        public string? GetAccessToken()
        {
            return _tokenStore.GetAccessToken();
        }

#if ANDROID
        private void StartAndroidService()
        {
            try
            {
                var context = Android.App.Application.Context;
                var intent = new Android.Content.Intent(context, typeof(Platforms.Android.Services.TokenService));
                context.StartForegroundService(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start service: {ex.Message}");
            }
        }

        private void StopAndroidService()
        {
            try
            {
                var context = Android.App.Application.Context;
                var intent = new Android.Content.Intent(context, typeof(Platforms.Android.Services.TokenService));
                context.StopService(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to stop service: {ex.Message}");
            }
        }
#endif
    }
}