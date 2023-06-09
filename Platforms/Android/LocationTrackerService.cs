using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.OS;
using Android.Runtime;
using Android.Util;
using MauiTrackTestSP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Platforms.Android
{
    [Service]
    public class LocationTrackerService : Service
    {
        private const string TAG = "LocationTrackerService";

        FusedLocationProviderClient moFusedLocationClient;
        LocationTrackingCallback moLocationTrackingCallback;
        private bool mbIsStarted = false;

        public static MainActivity MainActivityInstance { get; set; }
        public static LocationTracker LocationTrackerInstance { get; set; }


        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate: the service is initializing.");

            moFusedLocationClient = LocationServices.GetFusedLocationProviderClient(MainActivityInstance);

        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (mbIsStarted)
                {
                    Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
                    Log.Info(TAG, "OnStartCommand: The service is starting.");
                    RegisterForegroundService();
                    mbIsStarted = true;
                    StartTracking();
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopForeground(StopForegroundFlags.Remove);
                StopSelf();
                mbIsStarted = false;

            }

            return StartCommandResult.Sticky;
        }

        private async void StartTracking()
        {
            var loLocationRequest = LocationRequest.Create()
                .SetPriority(Priority.PriorityHighAccuracy)
                .SetInterval(15 * 1000)
                .SetFastestInterval(5 * 1000);
            if (moLocationTrackingCallback == null)
            {
                moLocationTrackingCallback = new LocationTrackingCallback(LocationTrackerInstance);
            }
            await moFusedLocationClient.RequestLocationUpdatesAsync(loLocationRequest, moLocationTrackingCallback);
        }

        private void StopTracking()
        {
            if (moLocationTrackingCallback != null)
            {
                moFusedLocationClient.RemoveLocationUpdatesAsync(moLocationTrackingCallback);
                moLocationTrackingCallback = null;
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedStamp() method.
            return null;
        }

        public override void OnDestroy()
        {
            Log.Info(TAG, "OnDestroy: The started service is shutting down.");

            // stop location updates
            StopTracking();

            // Remove the notification from the status bar.
            var loNotificationManager = (NotificationManager)GetSystemService(NotificationService);
            loNotificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            mbIsStarted = false;
            base.OnDestroy();
        }

        void RegisterForegroundService()
        {
            const string CHANNEL_ID = "Location_Tracker_Notifier";
            const string CHANNEL_DESC = "Location Tracker Notifier";
            Notification loNotification;
            var loNotificationManager = (NotificationManager)MainActivityInstance.GetSystemService(Context.NotificationService);
            if (loNotificationManager.GetNotificationChannel(CHANNEL_ID) == null)
            {
                var loChannel = new NotificationChannel(CHANNEL_ID, CHANNEL_DESC, NotificationImportance.High);
                loNotificationManager.CreateNotificationChannel(loChannel);
            }
            loNotification = new Notification.Builder(this, CHANNEL_ID)
              .SetContentTitle("Location Tracker")
              .SetContentText("Location is being tracked")
              .SetSmallIcon(Resource.Drawable.status_icon)
              .SetContentIntent(BuildIntentToShowMainActivity())
              .SetOngoing(true)
              .Build();


            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, loNotification);
        }

        PendingIntent BuildIntentToShowMainActivity()
        {
            var loNotificationIntent = new Intent(this, typeof(MainActivity));
            loNotificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
            loNotificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            loNotificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

            var loPendingIntent = PendingIntent.GetActivity(this, 0, loNotificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
            return loPendingIntent;
        }

    }
}
