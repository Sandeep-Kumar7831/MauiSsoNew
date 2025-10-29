#if ANDROID
using Android.Content;
using Android.Preferences;
using System;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// Token store that uses Android SharedPreferences for cross-process access
    /// This allows both the app and the service to access the same tokens
    /// </summary>
    public class SharedTokenStore : ITokenStore
    {
        private const string PREFS_NAME = "MauiSsoTokens";
        private const string ACCESS_TOKEN_KEY = "sso_access_token";
        private const string REFRESH_TOKEN_KEY = "sso_refresh_token";
        private const string ID_TOKEN_KEY = "sso_id_token";
        private const string EXPIRES_AT_KEY = "sso_expires_at";

        private readonly ISharedPreferences _prefs;

        public SharedTokenStore(Context context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Use MODE_PRIVATE for security but accessible within same app
            _prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);

            System.Diagnostics.Debug.WriteLine($"SharedTokenStore: Initialized with context: {context.GetType().Name}");
        }

        public string? GetAccessToken()
        {
            try
            {
                var token = _prefs.GetString(ACCESS_TOKEN_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: GetAccessToken - length: {token?.Length ?? 0}");

                if (!string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine($"SharedTokenStore: Token preview: {token.Substring(0, Math.Min(20, token.Length))}...");
                }

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

        public bool IsAuthenticated()
        {
            try
            {
                var accessToken = GetAccessToken();
                if (string.IsNullOrEmpty(accessToken))
                {
                    System.Diagnostics.Debug.WriteLine("SharedTokenStore: IsAuthenticated = false (no token)");
                    return false;
                }

                var expiresAtStr = _prefs.GetString(EXPIRES_AT_KEY, null);
                if (string.IsNullOrEmpty(expiresAtStr))
                {
                    System.Diagnostics.Debug.WriteLine("SharedTokenStore: IsAuthenticated = true (no expiry set)");
                    return true; // Has token but no expiry
                }

                if (DateTimeOffset.TryParse(expiresAtStr, out var expiresAt))
                {
                    var isValid = DateTimeOffset.UtcNow < expiresAt.AddMinutes(-5); // 5 min buffer
                    System.Diagnostics.Debug.WriteLine($"SharedTokenStore: IsAuthenticated = {isValid} (expires: {expiresAt})");
                    return isValid;
                }

                System.Diagnostics.Debug.WriteLine("SharedTokenStore: IsAuthenticated = true (invalid expiry format)");
                return true; // Has token but can't parse expiry
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
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveTokensAsync - Access token length: {tokens.AccessToken?.Length ?? 0}");

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

                // Verify save
                var savedToken = _prefs.GetString(ACCESS_TOKEN_KEY, null);
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: Verification - saved token length: {savedToken?.Length ?? 0}");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: SaveTokensAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SharedTokenStore: Stack trace: {ex.StackTrace}");
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