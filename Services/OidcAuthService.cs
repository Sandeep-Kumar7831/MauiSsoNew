using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using System;
using System.Threading.Tasks;

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
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Starting login...");

                var result = await _oidcClient.LoginAsync(new LoginRequest());

                if (result.IsError)
                {
                    System.Diagnostics.Debug.WriteLine($"OidcAuthService: Login error - {result.Error}");
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = result.Error,
                        ErrorDescription = result.ErrorDescription
                    };
                }

                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Login successful, access token length: {result.AccessToken?.Length ?? 0}");

                // Calculate expiry
                var expiresAt = result.AccessTokenExpiration;

                var tokenResponse = new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    IdToken = result.IdentityToken,
                    ExpiresAt = expiresAt
                };

                // Save tokens to secure storage
                await _tokenStore.SaveTokensAsync(tokenResponse);
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Tokens saved to secure storage");

                // Verify tokens were saved
                var savedToken = _tokenStore.GetAccessToken();
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Verification - saved token length: {savedToken?.Length ?? 0}");

//#if ANDROID
//                if (_config.EnableAndroidService)
//                {
//                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Starting Android service...");
//                    StartAndroidService();

//                    // Give service time to start
//                    await Task.Delay(1000);
//                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Android service start completed");
//                }
//#endif

                return new AuthResult
                {
                    IsSuccess = true,
                    AccessToken = result.AccessToken
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Exception during login - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Stack trace - {ex.StackTrace}");

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
                    System.Diagnostics.Debug.WriteLine("OidcAuthService: No refresh token available");
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = "NoRefreshToken"
                    };
                }

                System.Diagnostics.Debug.WriteLine("OidcAuthService: Refreshing token...");
                var result = await _oidcClient.RefreshTokenAsync(refreshToken);

                if (result.IsError)
                {
                    System.Diagnostics.Debug.WriteLine($"OidcAuthService: Refresh error - {result.Error}");
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
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Token refresh successful");

                return new AuthResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Refresh exception - {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Starting logout...");

                var idToken = _tokenStore.GetIdToken();
                if (!string.IsNullOrEmpty(idToken))
                {
                    await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = idToken });
                }

                _tokenStore.ClearTokens();
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Tokens cleared");

#if ANDROID
                // Note: We don't stop the service on logout
                // The service continues running to handle future logins
                // Only stop if explicitly requested
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Service remains running after logout");
#endif
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Logout error - {ex.Message}");
            }
        }

        public bool IsAuthenticated()
        {
            var result = _tokenStore.IsAuthenticated();
            System.Diagnostics.Debug.WriteLine($"OidcAuthService: IsAuthenticated = {result}");
            return result;
        }

        public string? GetAccessToken()
        {
            var token = _tokenStore.GetAccessToken();
            System.Diagnostics.Debug.WriteLine($"OidcAuthService: GetAccessToken - length: {token?.Length ?? 0}");
            return token;
        }

#if ANDROID
        private void StartAndroidService()
        {
            try
            {
                var context = Android.App.Application.Context;
                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Cannot start service - context is null");
                    return;
                }

                var intent = new Android.Content.Intent(context, typeof(Platforms.Android.Services.TokenService));

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                {
                    context.StartForegroundService(intent);
                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Started service with StartForegroundService");
                }
                else
                {
                    context.StartService(intent);
                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Started service with StartService");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Failed to start service - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Stack trace - {ex.StackTrace}");
            }
        }

        private void StopAndroidService()
        {
            try
            {
                var context = Android.App.Application.Context;
                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("OidcAuthService: Cannot stop service - context is null");
                    return;
                }

                var intent = new Android.Content.Intent(context, typeof(Platforms.Android.Services.TokenService));
                context.StopService(intent);
                System.Diagnostics.Debug.WriteLine("OidcAuthService: Service stop requested");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthService: Failed to stop service - {ex.Message}");
            }
        }
#endif
    }
}