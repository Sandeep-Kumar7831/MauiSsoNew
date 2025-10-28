#if ANDROID
using Android.Content;
using Android.OS;
using System;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    /// 
    /// Helper class to connect to the TokenService from client apps
    /// 
    public class TokenServiceConnection : Java.Lang.Object, IServiceConnection
    {
        private Com.Mauisso.Library.ITokenService? _service;
        private bool _isBound = false;
        private readonly Action? _onConnectionChanged;

        public bool IsBound => _isBound;

        public Com.Mauisso.Library.ITokenService? Service => _service;

        public TokenServiceConnection(Action? onConnectionChanged = null)
        {
            _onConnectionChanged = onConnectionChanged;
        }

        public void OnServiceConnected(ComponentName? name, IBinder? service)
        {
            System.Diagnostics.Debug.WriteLine("TokenServiceConnection: Service connected");

            if (service != null)
            {
                _service = Com.Mauisso.Library.ITokenServiceStub.AsInterface(service);
                _isBound = true;
                _onConnectionChanged?.Invoke();
            }
        }

        public void OnServiceDisconnected(ComponentName? name)
        {
            System.Diagnostics.Debug.WriteLine("TokenServiceConnection: Service disconnected");
            _service = null;
            _isBound = false;
            _onConnectionChanged?.Invoke();
        }

        public void Dispose()
        {
            _service = null;
            _isBound = false;
        }
    }
}
#endif