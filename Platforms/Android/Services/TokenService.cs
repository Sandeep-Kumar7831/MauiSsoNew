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
        private const string CHANNEL_ID = "MauiSsoChannel";
        private const int NOTIFICATION_ID = 9001;
        private TokenServiceStub? _binder;
        private ITokenStore? _tokenStore;

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                _tokenStore = IPlatformApplication.Current?.Services?.GetService<ITokenStore>();
                _binder = new TokenServiceStub(this);
                CreateNotificationChannel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TokenService OnCreate error: {ex.Message}");
            }
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return _binder?.AsBinder();
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            var notification = CreateNotification();
            StartForeground(NOTIFICATION_ID, notification);
            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    CHANNEL_ID,
                    "SSO Token Service",
                    NotificationImportance.Low)
                {
                    Description = "Manages authentication tokens"
                };

                var notificationManager = GetSystemService(NotificationService) as NotificationManager;
                notificationManager?.CreateNotificationChannel(channel);
            }
        }

        private Notification CreateNotification()
        {
            // Use library's resource or default Android icon
            int iconId = Resources?.GetIdentifier("notification_icon", "drawable", PackageName)
                ?? global::Android.Resource.Drawable.IcDialogInfo;

            var notificationBuilder = new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetContentTitle("SSO Service")
                .SetContentText("Authentication service running")
                .SetSmallIcon(iconId)
                .SetPriority(NotificationCompat.PriorityLow)
                .SetOngoing(true);

            return notificationBuilder.Build();
        }

        public string? GetAccessToken()
        {
            return _tokenStore?.GetAccessToken();
        }

        public string? GetRefreshToken()
        {
            return _tokenStore?.GetRefreshToken();
        }

        public string? GetIdToken()
        {
            return _tokenStore?.GetIdToken();
        }

        public bool IsAuthenticated()
        {
            return _tokenStore?.IsAuthenticated() ?? false;
        }

        public void Logout()
        {
            _tokenStore?.ClearTokens();
        }

        public override void OnDestroy()
        {
            StopForeground(StopForegroundFlags.Remove);
            base.OnDestroy();
        }
    }
}
#endif