#if ANDROID
using Android.OS;
using Android.Runtime;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    /// <summary>
    /// Implementation of the AIDL-generated ITokenService interface
    /// This class bridges between the AIDL interface and the actual TokenService
    /// </summary>
    public class TokenServiceStub : Com.Mauisso.Library.ITokenServiceStub
    {
        private readonly TokenService _service;

        public TokenServiceStub(TokenService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public override string GetAccessToken()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TokenServiceStub: GetAccessToken called");
                var token = _service.GetAccessToken();
                return token ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenServiceStub: GetAccessToken error: {ex.Message}");
                return string.Empty;
            }
        }

        public override string GetRefreshToken()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TokenServiceStub: GetRefreshToken called");
                var token = _service.GetRefreshToken();
                return token ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenServiceStub: GetRefreshToken error: {ex.Message}");
                return string.Empty;
            }
        }

        public override string GetIdToken()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TokenServiceStub: GetIdToken called");
                var token = _service.GetIdToken();
                return token ?? string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenServiceStub: GetIdToken error: {ex.Message}");
                return string.Empty;
            }
        }

        public override bool IsAuthenticated()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TokenServiceStub: IsAuthenticated called");
                return _service.IsAuthenticated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenServiceStub: IsAuthenticated error: {ex.Message}");
                return false;
            }
        }

        public override void Logout()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("TokenServiceStub: Logout called");
                _service.Logout();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenServiceStub: Logout error: {ex.Message}");
            }
        }
    }
}
#endif