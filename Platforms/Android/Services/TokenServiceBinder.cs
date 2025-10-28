#if ANDROID
using Android.OS;
using Android.Runtime;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    // After building, the generated class will be: Com.Mauisso.Library.ITokenService
    // Use the abstract Stub class that gets generated
    public class TokenServiceStub : Java.Lang.Object, global::Com.Mauisso.Library.ITokenService
    {
        private readonly TokenService _service;

        public TokenServiceStub(TokenService service)
        {
            _service = service;
        }

        public string GetAccessToken()
        {
            return _service.GetAccessToken() ?? string.Empty;
        }

        public string GetRefreshToken()
        {
            return _service.GetRefreshToken() ?? string.Empty;
        }

        public string GetIdToken()
        {
            return _service.GetIdToken() ?? string.Empty;
        }

        public bool IsAuthenticated()
        {
            return _service.IsAuthenticated();
        }

        public void Logout()
        {
            _service.Logout();
        }

        public IBinder AsBinder()
        {
            return this.JavaCast<IBinder>();
        }
    }
}
#endif