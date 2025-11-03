using Duende.IdentityModel.OidcClient;
using Duende.IdentityModel.OidcClient.Browser;
using Duende.IdentityModel.OidcClient.DPoP;
using System;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// OIDC Authentication Service with DPoP support using Duende library
    /// </summary>
    public class OidcAuthServiceDPoP : IOidcAuthService
    {
        private readonly OidcClient _oidcClient;
        private readonly ITokenStore _tokenStore;
        private readonly SsoConfiguration _config;
        private readonly IDPoPJwkManager _jwkManager;
        private string? _currentJwk;

        public OidcAuthServiceDPoP(
            ITokenStore tokenStore,
            SsoConfiguration config,
            IDPoPJwkManager jwkManager)
        {
            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _jwkManager = jwkManager ?? throw new ArgumentNullException(nameof(jwkManager));

            if (!config.IsValid())
                throw new ArgumentException("Invalid SSO configuration");

            System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Initializing with DPoP support");

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

            // Initialize DPoP - this is CRITICAL for DPoP to work
            InitializeDPoPAsync(options).Wait();

            _oidcClient = new OidcClient(options);
            System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: OidcClient created with DPoP enabled");
        }

        /// <summary>
        /// Initialize DPoP on OidcClientOptions - MUST be called before creating OidcClient
        /// </summary>
        private async Task InitializeDPoPAsync(OidcClientOptions options)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Initializing DPoP configuration");

                // Get or create JWK using DPoP library
                _currentJwk = await _jwkManager.GetOrCreateJwkAsync();

                if (string.IsNullOrEmpty(_currentJwk))
                    throw new InvalidOperationException("Failed to create DPoP JWK");

                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: DPoP JWK obtained (length: {_currentJwk.Length})");

                // Configure DPoP on the options BEFORE creating OidcClient
                // This enables DPoP for all subsequent operations
                options.ConfigureDPoP(_currentJwk);

                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: DPoP configured on OidcClientOptions");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: DPoP initialization error: {ex.Message}");
                throw;
            }
        }

        public async Task<AuthResult> LoginAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Starting login with DPoP...");

                var result = await _oidcClient.LoginAsync(new LoginRequest());

                if (result.IsError)
                {
                    System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Login error - {result.Error}");
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = result.Error,
                        ErrorDescription = result.ErrorDescription
                    };
                }

                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Login successful");

                var expiresAt = result.AccessTokenExpiration;

                var tokenResponse = new TokenResponse
                {
                    AccessToken = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    IdToken = result.IdentityToken,
                    ExpiresAt = expiresAt
                };

                await _tokenStore.SaveTokensAsync(tokenResponse);
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Tokens saved");

                var savedToken = _tokenStore.GetAccessToken();
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Verification - saved token length: {savedToken?.Length ?? 0}");

                return new AuthResult
                {
                    IsSuccess = true,
                    AccessToken = result.AccessToken
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Exception during login - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Stack trace - {ex.StackTrace}");

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
                    System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: No refresh token available");
                    return new AuthResult
                    {
                        IsSuccess = false,
                        Error = "NoRefreshToken"
                    };
                }

                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Refreshing token with DPoP...");

                var result = await _oidcClient.RefreshTokenAsync(refreshToken);

                if (result.IsError)
                {
                    System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Refresh error - {result.Error}");
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
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Token refresh successful");

                return new AuthResult { IsSuccess = true };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Refresh exception - {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Starting logout...");

                var idToken = _tokenStore.GetIdToken();
                if (!string.IsNullOrEmpty(idToken))
                {
                    await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = idToken });
                }

                _tokenStore.ClearTokens();
                await _jwkManager.ResetJwkAsync();

                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: Logout completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: Logout error - {ex.Message}");
            }
        }

        public bool IsAuthenticated()
        {
            var result = _tokenStore.IsAuthenticated();
            System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: IsAuthenticated = {result}");
            return result;
        }

        public string? GetAccessToken()
        {
            var token = _tokenStore.GetAccessToken();
            System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: GetAccessToken - length: {token?.Length ?? 0}");
            return token;
        }

        /// <summary>
        /// Get DPoP-aware HTTP handler for API calls
        /// </summary>
        public async Task<HttpMessageHandler> GetDPoPHandlerAsync(string? sessionRefreshToken = null)
        {
            try
            {
                // Ensure JWK exists or create new one
                var jwkString = await _jwkManager.GetOrCreateJwkAsync();

                // CreateDPoPHandler expects JWK as string
                var handler = _oidcClient.CreateDPoPHandler(jwkString, sessionRefreshToken);
                System.Diagnostics.Debug.WriteLine("OidcAuthServiceDPoP: DPoP handler created");
                return handler;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OidcAuthServiceDPoP: GetDPoPHandlerAsync error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Synchronous version - Get DPoP-aware HTTP handler for API calls
        /// </summary>
        public HttpMessageHandler GetDPoPHandler(string? sessionRefreshToken = null)
        {
            return GetDPoPHandlerAsync(sessionRefreshToken).Result;
        }
    }
}