using Duende.IdentityModel.OidcClient.DPoP;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// Manages DPoP JSON Web Key (JWK) lifecycle
    /// </summary>
    public interface IDPoPJwkManager
    {
        Task<string> GetOrCreateJwkAsync();
        Task<string> GetJwkAsync();
        Task ResetJwkAsync();
        bool HasJwk();
    }

    public class DPoPJwkManager : IDPoPJwkManager
    {
        private const string JWK_STORAGE_KEY = "dpop_jwk";
        private readonly ITokenStore _tokenStore;
        private string? _cachedJwk;

        public DPoPJwkManager(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        }

        /// <summary>
        /// Get existing JWK or create a new one
        /// </summary>
        public async Task<string> GetOrCreateJwkAsync()
        {
            try
            {
                // Try to get cached JWK
                if (!string.IsNullOrEmpty(_cachedJwk))
                {
                    System.Diagnostics.Debug.WriteLine("DPoPJwkManager: Using cached JWK");
                    return _cachedJwk;
                }

                // Try to get from storage
                var existingJwk = _tokenStore.GetDPoPJwk();
                if (!string.IsNullOrEmpty(existingJwk))
                {
                    _cachedJwk = existingJwk;
                    System.Diagnostics.Debug.WriteLine("DPoPJwkManager: Retrieved JWK from storage");
                    return existingJwk;
                }

                // Create new JWK using Duende library
                System.Diagnostics.Debug.WriteLine("DPoPJwkManager: Creating new RS256 JWK");
                var newJwk = JsonWebKeys.CreateRsaJson();

                // Cache it
                _cachedJwk = newJwk;

                // Store it
                _tokenStore.SaveDPoPJwk(newJwk);
                System.Diagnostics.Debug.WriteLine("DPoPJwkManager: New JWK created and stored");

                await Task.CompletedTask;
                return newJwk;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DPoPJwkManager: GetOrCreateJwkAsync error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get stored JWK
        /// </summary>
        public async Task<string> GetJwkAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cachedJwk))
                    return _cachedJwk;

                var jwk = _tokenStore.GetDPoPJwk();
                if (!string.IsNullOrEmpty(jwk))
                {
                    _cachedJwk = jwk;
                    await Task.CompletedTask;
                    return jwk;
                }

                throw new InvalidOperationException("No JWK found. Call GetOrCreateJwkAsync first.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DPoPJwkManager: GetJwkAsync error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reset the JWK (for logout)
        /// </summary>
        public async Task ResetJwkAsync()
        {
            try
            {
                _cachedJwk = null;
                System.Diagnostics.Debug.WriteLine("DPoPJwkManager: JWK reset");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DPoPJwkManager: ResetJwkAsync error: {ex.Message}");
            }
        }

        public bool HasJwk()
        {
            return !string.IsNullOrEmpty(_cachedJwk);
        }
    }
}
