#if ANDROID
using Android.Content;
using Android.Preferences;
using System;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// Token store that uses Android SharedPreferences with DPoP JWK support
    /// </summary>
    public class SharedTokenStore : ITokenStore
    {
        private const string PREFS_NAME = "MauiSsoTokens";
        private const string ACCESS_TOKEN_KEY = "sso_access_token";
        private const string REFRESH_TOKEN_KEY = "sso_refresh_token";
        private const string ID_TOKEN_KEY = "sso_id_token";
        private const string EXPIRES_AT_KEY = "sso_expires_at";
        private const string DPOP_JWK_KEY = "sso_dpop_jwk";

        private readonly ISharedPreferences _prefs;

        public SharedTokenStore(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            System.Diagnostics.Debug.WriteLine($"SharedTokenStore: Initialized with DPoP JWK support");
        }

        public string? GetAccessToken()
        {
            try
            {
                var token = _prefs.GetString(ACCESS_TOKEN_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetAccessToken - length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetAccessToken error: {ex.Message}");
                return null;
            }
        }

        public string? GetRefreshToken()
        {
            try
            {
                var token = _prefs.GetString(REFRESH_TOKEN_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetRefreshToken - length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetRefreshToken error: {ex.Message}");
                return null;
            }
        }

        public string? GetIdToken()
        {
            try
            {
                var token = _prefs.GetString(ID_TOKEN_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetIdToken - length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetIdToken error: {ex.Message}");
                return null;
            }
        }

        public string? GetDPoPJwk()
        {
            try
            {
                var jwk = _prefs.GetString(DPOP_JWK_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetDPoPJwk - present: {!string.IsNullOrEmpty(jwk)}");
                return jwk;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetDPoPJwk error: {ex.Message}");
                return null;
            }
        }

        public void SaveDPoPJwk(string jwkJson)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SharedTokenStore: Saving DPoP JWK");
                var editor = _prefs.Edit();
                editor.PutString(DPOP_JWK_KEY, jwkJson);
                var success = editor.Commit();
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveDPoPJwk - Commit result: {success}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveDPoPJwk error: {ex.Message}");
                throw;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                var accessToken = GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    System.Diagnostics.Debug.WriteLine("SharedTokenStore: IsAuthenticated = false");
                    return false;
                }

                var expiresAtStr = _prefs.GetString(EXPIRES_AT_KEY, null);
                if (string.IsNullOrEmpty(expiresAtStr))
                    return true;

                if (DateTimeOffset.TryParse(expiresAtStr, out var expiresAt))
                {
                    var isValid = DateTimeOffset.UtcNow < expiresAt.AddMinutes(-5);
                    System.Diagnostics.Debug.WriteLine($"SharedTokenStore: IsAuthenticated = {isValid}");
                    return isValid;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: IsAuthenticated error: {ex.Message}");
                return false;
            }
        }

        public async Task SaveTokensAsync(TokenResponse tokens)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveTokensAsync");

                var editor = _prefs.Edit();

                if (!string.IsNullOrEmpty(tokens.AccessToken))
                    editor.PutString(ACCESS_TOKEN_KEY, tokens.AccessToken);

                if (!string.IsNullOrEmpty(tokens.RefreshToken))
                    editor.PutString(REFRESH_TOKEN_KEY, tokens.RefreshToken);

                if (!string.IsNullOrEmpty(tokens.IdToken))
                    editor.PutString(ID_TOKEN_KEY, tokens.IdToken);

                editor.PutString(EXPIRES_AT_KEY, tokens.ExpiresAt.ToString("O"));

                var success = editor.Commit();
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveTokensAsync - Commit result: {success}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveTokensAsync error: {ex.Message}");
                throw;
            }
        }

        public void ClearTokens()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SharedTokenStore: ClearTokens called");

                var editor = _prefs.Edit();
                editor.Remove(ACCESS_TOKEN_KEY);
                editor.Remove(REFRESH_TOKEN_KEY);
                editor.Remove(ID_TOKEN_KEY);
                editor.Remove(EXPIRES_AT_KEY);
                editor.Remove(DPOP_JWK_KEY);

                var success = editor.Commit();
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: ClearTokens - Commit result: {success}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: ClearTokens error: {ex.Message}");
            }
        }
    }
}
#endif