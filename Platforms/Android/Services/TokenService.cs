#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MauiSsoLibrary.Services;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    [Service(Name = "com.mauisso.library.TokenService",
             Exported = true,
             Enabled = true)]
    [IntentFilter(new[] { "com.mauisso.library.ITokenService" })]
    public class TokenService : Service
    {
        private const string CHANNEL_ID = "MauiSsoServiceChannel";
        private const int NOTIFICATION_ID = 9001;
        private TokenServiceStub? _binder;
        private ITokenStore? _tokenStore;
        private bool _isRunning = false;

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                // Create token store directly
                _tokenStore = new TokenStore();
                _binder = new TokenServiceStub(this);
                CreateNotificationChannel();

                System.Diagnostics.Debug.WriteLine("TokenService: OnCreate completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService OnCreate error: {ex.Message}");
            }
        }

        public override IBinder? OnBind(Intent? intent)
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnBind called");

            if (!_isRunning)
            {
                StartAsForeground();
            }

            return _binder?.AsBinder();
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnStartCommand called");

            if (!_isRunning)
            {
                StartAsForeground();
            }

            return StartCommandResult.Sticky;
        }

        private void StartAsForeground()
        {
            try
            {
                var notification = CreateNotification();

                // Use different method based on API level
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    // API 29+ (Android 10+)
                    // For API 34+, you would add foreground service type
                    // StartForeground(NOTIFICATION_ID, notification, ForegroundService.TypeDataSync);
                    StartForeground(NOTIFICATION_ID, notification);
                }
                else
                {
                    // API 26-28
                    StartForeground(NOTIFICATION_ID, notification);
                }

                _isRunning = true;
                System.Diagnostics.Debug.WriteLine("TokenService: Running as foreground service");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: StartAsForeground error: {ex.Message}");
            }
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "SSO Authentication Service",
                    NotificationImportance.Low)
                {
                    Description = "Manages secure authentication tokens for applications"
                };

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        private Notification CreateNotification()
        {
            int iconId = global::Android.Resource.Drawable.IcDialogInfo;

            var intent = new Intent();
            var pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                intent,
                Build.VERSION.SdkInt >= BuildVersionCodes.M
                    ? PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("SSO Service Active")
                .SetContentText("Authentication service is running")
                .SetSmallIcon(iconId)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetOngoing(true)
                .SetContentIntent(pendingIntent)
                .SetCategory(NotificationCompat.CategoryService);

            return notificationBuilder.Build();
        }

        public string? GetAccessToken()
        {
            try
            {
                var token = _tokenStore?.GetAccessToken();
                System.Diagnostics.Debug.WriteLine($"TokenService: GetAccessToken called, token length: {token?.Length ?? 0}");
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: GetAccessToken error: {ex.Message}");
                return null;
            }
        }

        public string? GetRefreshToken()
        {
            try
            {
                return _tokenStore?.GetRefreshToken();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: GetRefreshToken error: {ex.Message}");
                return null;
            }
        }

        public string? GetIdToken()
        {
            try
            {
                return _tokenStore?.GetIdToken();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: GetIdToken error: {ex.Message}");
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            try
            {
                var result = _tokenStore?.IsAuthenticated() ?? false;
                System.Diagnostics.Debug.WriteLine($"TokenService: IsAuthenticated = {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: IsAuthenticated error: {ex.Message}");
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                _tokenStore?.ClearTokens();
                System.Diagnostics.Debug.WriteLine("TokenService: Logout completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: Logout error: {ex.Message}");
            }
        }

        public override bool OnUnbind(Intent? intent)
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnUnbind called");
            return true;
        }

        public override void OnRebind(Intent? intent)
        {
            base.OnRebind(intent);
            System.Diagnostics.Debug.WriteLine("TokenService: OnRebind called");
        }

        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnDestroy called");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                StopForeground(StopForegroundFlags.Remove);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                StopForeground(true);
#pragma warning restore CS0618
            }

            _isRunning = false;
            base.OnDestroy();
        }
    }
}
#endif