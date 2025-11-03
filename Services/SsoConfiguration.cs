// 6. Configuration - Services/SsoConfiguration.cs
namespace MauiSsoLibrary.Services
{
    public class SsoConfiguration
    {
        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string? ClientSecret { get; set; }
        public string Scope { get; set; } = "openid profile email offline_access";
        public string RedirectUri { get; set; } = string.Empty;
        public string? PostLogoutRedirectUri { get; set; }
        public bool EnableAndroidService { get; set; } = true;
        public bool EnableDPoP { get; set; } = true;

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Authority) &&
                   !string.IsNullOrWhiteSpace(ClientId) &&
                   !string.IsNullOrWhiteSpace(RedirectUri);
        }
    }
}