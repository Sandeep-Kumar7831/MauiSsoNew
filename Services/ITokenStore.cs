using System;
using System.Threading.Tasks;

namespace MauiSsoLibrary.Services
{
    public interface ITokenStore
    {
        string? GetAccessToken();
        string? GetRefreshToken();
        string? GetIdToken();
        bool IsAuthenticated();
        Task SaveTokensAsync(TokenResponse tokens);
        void ClearTokens();
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string IdToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}