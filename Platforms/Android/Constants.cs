using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Platforms.Android
{
    public static class Constants
    {
        public const int SERVICE_RUNNING_NOTIFICATION_ID = 10000;
        public const string SERVICE_STARTED_KEY = "has_service_been_started";
        public const string BROADCAST_MESSAGE_KEY = "broadcast_message";
        public const string NOTIFICATION_BROADCAST_ACTION = "XamTrackTest.Droid.Notification.Action";

        public const string ACTION_START_SERVICE = "XamTrackTest.Droid.action.START_SERVICE";
        public const string ACTION_STOP_SERVICE = "XamTrackTest.Droid.action.STOP_SERVICE";
        public const string ACTION_MAIN_ACTIVITY = "XamTrackTest.Droid.action.MAIN_ACTIVITY";
    }
}
