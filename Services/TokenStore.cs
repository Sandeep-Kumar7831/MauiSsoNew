using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    public class TokenStore : ITokenStore
    {
        private const string ACCESS_TOKEN_KEY = "sso_access_token";
        private const string REFRESH_TOKEN_KEY = "sso_refresh_token";
        private const string ID_TOKEN_KEY = "sso_id_token";
        private const string EXPIRES_AT_KEY = "sso_expires_at";
        private const string DPOP_JWK_KEY = "sso_dpop_jwk";

        public string? GetAccessToken()
        {
            try
            {
                return SecureStorage.GetAsync(ACCESS_TOKEN_KEY).Result;
            }
            catch
            {
                return null;
            }
        }

        public string? GetRefreshToken()
        {
            try
            {
                return SecureStorage.GetAsync(REFRESH_TOKEN_KEY).Result;
            }
            catch
            {
                return null;
            }
        }

        public string? GetIdToken()
        {
            try
            {
                return SecureStorage.GetAsync(ID_TOKEN_KEY).Result;
            }
            catch
            {
                return null;
            }
        }

        public string? GetDPoPJwk()
        {
            try
            {
                return SecureStorage.GetAsync(DPOP_JWK_KEY).Result;
            }
            catch
            {
                return null;
            }
        }

        public void SaveDPoPJwk(string jwkJson)
        {
            try
            {
                SecureStorage.SetAsync(DPOP_JWK_KEY, jwkJson).Wait();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenStore: SaveDPoPJwk error: {ex.Message}");
                throw;
            }
        }

        public bool IsAuthenticated()
        {
            var accessToken = GetAccessToken();
            if (string.IsNullOrEmpty(accessToken))
                return false;

            try
            {
                var expiresAtStr = SecureStorage.GetAsync(EXPIRES_AT_KEY).Result;
                if (DateTime.TryParse(expiresAtStr, out var expiresAt))
                {
                    return DateTime.UtcNow < expiresAt.AddMinutes(-5);
                }
            }
            catch { }

            return false;
        }

        public async Task SaveTokensAsync(TokenResponse tokens)
        {
            await SecureStorage.SetAsync(ACCESS_TOKEN_KEY, tokens.AccessToken);
            await SecureStorage.SetAsync(REFRESH_TOKEN_KEY, tokens.RefreshToken);
            await SecureStorage.SetAsync(ID_TOKEN_KEY, tokens.IdToken);
            await SecureStorage.SetAsync(EXPIRES_AT_KEY, tokens.ExpiresAt.ToString("O"));
        }

        public void ClearTokens()
        {
            SecureStorage.Remove(ACCESS_TOKEN_KEY);
            SecureStorage.Remove(REFRESH_TOKEN_KEY);
            SecureStorage.Remove(ID_TOKEN_KEY);
            SecureStorage.Remove(EXPIRES_AT_KEY);
            SecureStorage.Remove(DPOP_JWK_KEY);
        }
    }
}


