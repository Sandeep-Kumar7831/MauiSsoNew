namespace MauiSsoLibrary.Services
{
    /// <summary>
    /// Configuration for SSO authentication
    /// </summary>
    public class SsoConfiguration
    {
        /// <summary>
        /// Identity Server authority URL
        /// </summary>
        public string Authority { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Client ID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// OAuth Client Secret (if required)
        /// </summary>
        public string? ClientSecret { get; set; }

        /// <summary>
        /// OAuth Scopes
        /// </summary>
        public string Scope { get; set; } = "openid profile email offline_access";

        /// <summary>
        /// Redirect URI for callback
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Post-logout redirect URI
        /// </summary>
        public string? PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Enable Android background service
        /// </summary>
        public bool EnableAndroidService { get; set; } = true;

        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Authority) &&
                   !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(RedirectUri);
        }
    }
}