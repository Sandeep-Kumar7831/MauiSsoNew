#if ANDROID
using Android.Content;
using System;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    /// 
    /// Client helper to interact with the TokenService
    /// Use this class from your MAUI app to connect to the service
    /// 
    public class SsoServiceClient : IDisposable
    {
        private readonly Context _context;
        private TokenServiceConnection? _connection;
        private bool _isBinding = false;

        public bool IsConnected => _connection?.IsBound ?? false;

        public event EventHandler? ConnectionChanged;

        public SsoServiceClient(Context context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// 
        /// Connect to the SSO service. Service will start if not running.
        /// 
        public bool Connect()
        {
            if (_isBinding || IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Already connected or connecting");
                return IsConnected;
            }

            try
            {
                _isBinding = true;

                // Create connection
                _connection = new TokenServiceConnection(() =>
                {
                    _isBinding = false;
                    ConnectionChanged?.Invoke(this, EventArgs.Empty);
                });

                // Create intent to bind to service
                var intent = new Intent("com.mauisso.library.ITokenService");
                intent.SetPackage("com.honeywell.mauissotestapp"); // Your app package name

                // Bind to service (this will start it if not running)
                var flags = Bind.AutoCreate;
                bool bound = _context.BindService(intent, _connection, flags);

                if (!bound)
                {
                    System.Diagnostics.Debug.WriteLine("SsoServiceClient: Failed to bind to service");
                    _isBinding = false;
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Binding to service...");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Connect error: {ex.Message}");
                _isBinding = false;
                return false;
            }
        }

        /// 
        /// Disconnect from the service (service continues running)
        /// 
        public void Disconnect()
        {
            if (_connection != null && IsConnected)
            {
                try
                {
                    _context.UnbindService(_connection);
                    System.Diagnostics.Debug.WriteLine("SsoServiceClient: Disconnected from service");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Disconnect error: {ex.Message}");
                }
                finally
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        /// 
        /// Get access token from service
        /// 
        public string? GetAccessToken()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected to service");
                return null;
            }

            try
            {
                return _connection.Service.GetAccessToken();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetAccessToken error: {ex.Message}");
                return null;
            }
        }

        /// 
        /// Get refresh token from service
        /// 
        public string? GetRefreshToken()
        {
            if (!IsConnected || _connection?.Service == null)
                return null;

            try
            {
                return _connection.Service.GetRefreshToken();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetRefreshToken error: {ex.Message}");
                return null;
            }
        }

        /// 
        /// Get ID token from service
        /// 
        public string? GetIdToken()
        {
            if (!IsConnected || _connection?.Service == null)
                return null;

            try
            {
                return _connection.Service.GetIdToken();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetIdToken error: {ex.Message}");
                return null;
            }
        }

        /// 
        /// Check if user is authenticated
        /// 
        public bool IsAuthenticated()
        {
            if (!IsConnected || _connection?.Service == null)
                return false;

            try
            {
                return _connection.Service.IsAuthenticated();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: IsAuthenticated error: {ex.Message}");
                return false;
            }
        }

        /// 
        /// Logout and clear tokens
        /// 
        public void Logout()
        {
            if (!IsConnected || _connection?.Service == null)
                return;

            try
            {
                _connection.Service.Logout();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Logout error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
#endif