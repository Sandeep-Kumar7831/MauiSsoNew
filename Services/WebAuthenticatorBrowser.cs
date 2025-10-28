using Duende.IdentityModel.OidcClient.Browser;

namespace MauiSsoLibrary.Services
{
    internal class WebAuthenticatorBrowser : Duende.IdentityModel.OidcClient.Browser.IBrowser
    {
        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await WebAuthenticator.AuthenticateAsync(
                    new Uri(options.StartUrl),
                    new Uri(options.EndUrl));

                var url = result.Properties.ContainsKey("url")
                    ? result.Properties["url"]
                    : options.EndUrl;

                return new BrowserResult
                {
                    Response = url,
                    ResultType = BrowserResultType.Success
                };
            }
            catch (TaskCanceledException)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UserCancel
                };
            }
            catch (Exception ex)
            {
                return new BrowserResult
                {
                    ResultType = BrowserResultType.UnknownError,
                    Error = ex.Message
                };
            }
        }
    }
}