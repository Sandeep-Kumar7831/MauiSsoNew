using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient.Browser;
using Microsoft.Maui.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

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


                var url = new RequestUrl("cfauth://com.honeywell.tools.honeywelllauncher/callback")
                    .Create(new Parameters(result.Properties));

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