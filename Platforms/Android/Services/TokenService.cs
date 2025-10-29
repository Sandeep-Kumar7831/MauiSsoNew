#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MauiSsoLibrary.Services;
using System;

namespace MauiSsoLibrary.Platforms.Android.Services
{
    [Service(Name = "com.mauisso.library.TokenService",
             Exported = true,
             Enabled = true,
             ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
    [IntentFilter(new[] { "com.mauisso.library.ITokenService" })]
    public class TokenService : Service
    {
        private const string CHANNEL_ID = "MauiSsoServiceChannel";
        private const int NOTIFICATION_ID = 9001;
        private TokenServiceStub? _binder;
        private ITokenStore? _tokenStore;
        private bool _isRunning = false;

        // Singleton instance to keep service alive
        private static TokenService? _instance;

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                _instance = this;

                // Use SecureStorage-backed TokenStore
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

            // Return STICKY to restart service if killed
            return StartCommandResult.Sticky;
        }

        private void StartAsForeground()
        {
            try
            {
                var notification = CreateNotification();

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
                    // API 29+ with foreground service type
                    StartForeground(NOTIFICATION_ID, notification,
                        global::Android.Content.PM.ForegroundService.TypeDataSync);
                }
                else
                {
                    StartForeground(NOTIFICATION_ID, notification);
                }

                _isRunning = true;
                System.Diagnostics.Debug.WriteLine("TokenService: Running as foreground service with STICKY mode");
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
                    NotificationImportance.Default) // Changed to Default for visibility
                {
                    Description = "Manages secure authentication tokens for applications",
                    LockscreenVisibility = NotificationVisibility.Public
                };

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);

                System.Diagnostics.Debug.WriteLine("TokenService: Notification channel created");
            }
        }

        private Notification CreateNotification()
        {
            int iconId = global::Android.Resource.Drawable.IcDialogInfo;

            // Create an intent to launch the app when notification is tapped
            var packageName = ApplicationContext?.PackageName;
            Intent? launchIntent = null;

            if (!string.IsNullOrEmpty(packageName))
            {
                launchIntent = PackageManager?.GetLaunchIntentForPackage(packageName);
            }

            PendingIntent? pendingIntent = null;
            if (launchIntent != null)
            {
                pendingIntent = PendingIntent.GetActivity(
                    this,
                    0,
                    launchIntent,
                    Build.VERSION.SdkInt >= BuildVersionCodes.M
                        ? PendingIntentFlags.Immutable
                        : PendingIntentFlags.UpdateCurrent);
            }

            var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("SSO Service Active")
                .SetContentText("Authentication service is running")
                .SetSmallIcon(iconId)
                .SetPriority(NotificationCompat.PriorityDefault) // Changed to Default
                .SetOngoing(true)
                .SetCategory(NotificationCompat.CategoryService)
                .SetVisibility(NotificationCompat.VisibilityPublic);

            if (pendingIntent != null)
            {
                notificationBuilder.SetContentIntent(pendingIntent);
            }

            return notificationBuilder.Build();
        }

        public string? GetAccessToken()
        {
            try
            {
                var token = _tokenStore?.GetAccessToken();
                var length = token?.Length ?? 0;
                System.Diagnostics.Debug.WriteLine($"TokenService: GetAccessToken - Retrieved token length: {length}");

                if (length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"TokenService: Token preview: {token?.Substring(0, Math.Min(20, length))}...");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("TokenService: No token found in secure storage");
                }

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: GetAccessToken error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TokenService: Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public string? GetRefreshToken()
        {
            try
            {
                var token = _tokenStore?.GetRefreshToken();
                System.Diagnostics.Debug.WriteLine($"TokenService: GetRefreshToken - length: {token?.Length ?? 0}");
                return token;
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
                var token = _tokenStore?.GetIdToken();
                System.Diagnostics.Debug.WriteLine($"TokenService: GetIdToken - length: {token?.Length ?? 0}");
                return token;
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
                var hasAccessToken = !string.IsNullOrEmpty(_tokenStore?.GetAccessToken());

                System.Diagnostics.Debug.WriteLine($"TokenService: IsAuthenticated = {result}");
                System.Diagnostics.Debug.WriteLine($"TokenService: Has access token = {hasAccessToken}");

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
                System.Diagnostics.Debug.WriteLine("TokenService: Logout completed - tokens cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService: Logout error: {ex.Message}");
            }
        }

        public override bool OnUnbind(Intent? intent)
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnUnbind - service continues running");

            // Return true to allow rebinding
            // Service keeps running in foreground
            return true;
        }

        public override void OnRebind(Intent? intent)
        {
            base.OnRebind(intent);
            System.Diagnostics.Debug.WriteLine("TokenService: OnRebind called");
        }

        public override void OnTaskRemoved(Intent? rootIntent)
        {
            // This is called when app is swiped away from recent apps
            System.Diagnostics.Debug.WriteLine("TokenService: OnTaskRemoved - keeping service alive");

            // Restart the service to keep it running
            var intent = new Intent(ApplicationContext, typeof(TokenService));
            var pendingIntent = PendingIntent.GetService(
                ApplicationContext,
                1,
                intent,
                Build.VERSION.SdkInt >= BuildVersionCodes.M
                    ? PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent);

            var alarmManager = GetSystemService(AlarmService) as AlarmManager;
            alarmManager?.Set(
                AlarmType.ElapsedRealtime,
                SystemClock.ElapsedRealtime() + 1000,
                pendingIntent);

            base.OnTaskRemoved(rootIntent);
        }

        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("TokenService: OnDestroy - attempting to restart");

            // Try to restart the service
            var broadcastIntent = new Intent();
            broadcastIntent.SetAction("com.mauisso.library.RestartService");
            broadcastIntent.SetClass(this, typeof(ServiceRestartReceiver));
            SendBroadcast(broadcastIntent);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                StopForeground(StopForegroundFlags.Detach);
            }
            else
            {
#pragma warning disable CS0618
                StopForeground(false);
#pragma warning restore CS0618
            }

            _isRunning = false;
            _instance = null;

            base.OnDestroy();
        }

        // Broadcast receiver to restart service
        [BroadcastReceiver(Enabled = true, Exported = false)]
        [IntentFilter(new[] { "com.mauisso.library.RestartService" })]
        public class ServiceRestartReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context? context, Intent? intent)
            {
                System.Diagnostics.Debug.WriteLine("ServiceRestartReceiver: Restarting TokenService");

                if (context != null)
                {
                    var serviceIntent = new Intent(context, typeof(TokenService));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        context.StartForegroundService(serviceIntent);
                    }
                    else
                    {
                        context.StartService(serviceIntent);
                    }
                }
            }
        }
    }
}
#endif