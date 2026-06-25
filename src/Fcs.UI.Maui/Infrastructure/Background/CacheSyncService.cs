#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;

namespace Fcs.UI.Maui.Infrastructure.Background;

[Service]
public class CacheSyncService : Service
{
    private const int NotificationId = 1001;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(15);
    private Timer? _timer;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        var channelId = "cache_sync_channel";
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(channelId, "Sincronização de Cache", NotificationImportance.Low);
            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }

        var notification = new Notification.Builder(this, channelId)
            .SetContentTitle("Conexão Solidária")
            .SetContentText("Sincronizando dados...")
            .SetSmallIcon(global::Android.Resource.Drawable.IcMenuUpload)
            .Build();

        StartForeground(NotificationId, notification);

        _timer = new Timer(async _ => await SyncCache(), null, TimeSpan.Zero, SyncInterval);
        return StartCommandResult.Sticky;
    }

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnDestroy()
    {
        _timer?.Dispose();
        base.OnDestroy();
    }

    private Task SyncCache()
    {
        return Task.CompletedTask;
    }
}
#endif