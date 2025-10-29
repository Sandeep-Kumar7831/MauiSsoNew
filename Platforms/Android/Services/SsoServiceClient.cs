#if ANDROID
using Android.Content;
using System;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    /// <summary>
    /// Client helper to interact with the TokenService
    /// Use this class from your MAUI app to connect to the service
    /// </summary>
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

        /// <summary>
        /// Connect to the SSO service. Service will start if not running.
        /// </summary>
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

                // IMPORTANT: Use the current app's package name, not a hardcoded one
                var packageName = _context.PackageName;
                intent.SetPackage(packageName);

                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Attempting to bind to service in package: {packageName}");

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
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Stack trace: {ex.StackTrace}");
                _isBinding = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the service (service continues running)
        /// </summary>
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
                    ConnectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Get access token from service
        /// </summary>
        public string? GetAccessToken()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected to service");
                return null;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Requesting access token from service...");
                var token = _connection.Service.GetAccessToken();
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Received token from service, length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetAccessToken error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Get refresh token from service
        /// </summary>
        public string? GetRefreshToken()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected to service");
                return null;
            }

            try
            {
                var token = _connection.Service.GetRefreshToken();
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Refresh token length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetRefreshToken error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get ID token from service
        /// </summary>
        public string? GetIdToken()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected to service");
                return null;
            }

            try
            {
                var token = _connection.Service.GetIdToken();
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: ID token length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: GetIdToken error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected - cannot check auth");
                return false;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Checking authentication status...");
                var result = _connection.Service.IsAuthenticated();
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Authentication status from service: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: IsAuthenticated error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Logout and clear tokens
        /// </summary>
        public void Logout()
        {
            if (!IsConnected || _connection?.Service == null)
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Not connected - cannot logout");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Sending logout request to service...");
                _connection.Service.Logout();
                System.Diagnostics.Debug.WriteLine("SsoServiceClient: Logout completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Logout error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SsoServiceClient: Stack trace: {ex.StackTrace}");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
#endif