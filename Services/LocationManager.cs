using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Services
{
    class LocationManager
    {
        enum GpsErrorCodes : int
        {
            UnkownLocation = 0,
            UserDenied = 1,
            Network = 2,
            HeadingFailure = 3
        }

        private static LocationTracker moLocationTracker;
        private static TaskCompletionSource<GpsData> moCurrentLocationTaskCompletionSource;
        private static DateTime mdLastReported;
        
        public static TimeSpan ReportInterval { get; set; }

        #region  Events 

        public delegate void GpsDataArrivedEventHandler(GpsDataArrivedEventArgs e);
        public static event GpsDataArrivedEventHandler GpsDataArrived;

        protected static void OnGpsDataArrived(GpsDataArrivedEventArgs e)
        {
            GpsDataArrived?.Invoke(e);
        }

        #endregion

        private static LocationTracker GetLocationTracker()
        {
            try
            {
                if (moLocationTracker == null)
                {
                    moLocationTracker = new();
                }
                return moLocationTracker;
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Get Current Location", ex.Message, "OK");
            }
            return null;
        }

        public static async Task<bool> IsLocationServicesAvailableAsync(bool always)
        {
            try
            {
                Console.WriteLine("IsLocationServicesAvailableAsync");
                //var lbLiveTrackingEnabled = Settings.GetSetting<bool>("LiveTrackingEnabled", false);

                // check current permission status
                PermissionStatus loPermissionStatus;
                if (always)
                {
                    loPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
                    Console.WriteLine($"Check Permissions.LocationAlways={loPermissionStatus}");
                }
                else
                {
                    loPermissionStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    Console.WriteLine($"Check Permissions.LocationWhenInUse={loPermissionStatus}");
                }

                // if already granted, all is good
                if (loPermissionStatus == PermissionStatus.Granted)
                {
                    Console.WriteLine($"Permissions Granted");
                    return true;
                }

                // on iOS we can only have the OS request permission once, it will be set to Denied if they don't grant permission
                if (loPermissionStatus == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    // prompt the user to turn on location services in Settings
                    await App.Current.MainPage.DisplayAlert("Location Required", "Location Services Not Available", "OK");
                    return false;
                }

                // have the OS request permission for location services
                if (always)
                {
                    loPermissionStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
                    Console.WriteLine($"Request Permissions.LocationAlways={loPermissionStatus}");
                }
                else
                {
                    loPermissionStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    Console.WriteLine($"Request Permissions.LocationWhenInUse={loPermissionStatus}");
                }
                Console.WriteLine($"Permission Granted={loPermissionStatus == PermissionStatus.Granted}");
                return (loPermissionStatus == PermissionStatus.Granted);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Is Location Services Enabled", ex.Message, "OK");
                });
            }
            return false;
        }

        public static async Task<GpsData> GetCurrentLocationAsync()
        {
            try
            {
                var loLocationTracker = GetLocationTracker();

                // make sure location services are enabled and allowed
                var lbLocationServicesAvailable = await IsLocationServicesAvailableAsync(false);
                if (!lbLocationServicesAvailable)
                {
                    await App.Current.MainPage.DisplayAlert("Get Current Location", "Location service not enabled or not authorized.", "OK");
                    return null;
                }

                return await WaitForCurrentLocation(loLocationTracker);

            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Get Current Location", ex.Message, "OK");
                });
            }
            return null;
        }

        private static Task<GpsData> WaitForCurrentLocation(LocationTracker locationTracker)
        {
            // make sure concurrent calls are not executed
            if (moCurrentLocationTaskCompletionSource != null)
            {
                return new Task<GpsData>(() => new GpsData()
                {
                    Error = new GpsData.ErrorInfo()
                    {
                        ID = -1,
                        Message = "A request for current location is already pending."
                    }
                });
            }

            // Ask for location and await its response
            moCurrentLocationTaskCompletionSource = new TaskCompletionSource<GpsData>();
            locationTracker.CurrentLocationUpdated += LocationTracker_CurrentLocationUpdated;
            locationTracker.CurrentLocationFailed += LocationTracker_CurrentLocationFailed;
            locationTracker.GetCurrentLocation();
            return moCurrentLocationTaskCompletionSource.Task;
        }

        private static void LocationTracker_CurrentLocationUpdated(object sender, GpsData e)
        {
            try
            {
                if (moCurrentLocationTaskCompletionSource != null)
                {
                    moCurrentLocationTaskCompletionSource.SetResult(e);
                }
                System.Diagnostics.Debug.WriteLine($"Current location changed: {DateTime.Now}");

                var loLocationTracker = GetLocationTracker();
                loLocationTracker.CurrentLocationUpdated -= LocationTracker_CurrentLocationUpdated;
                loLocationTracker.CurrentLocationFailed -= LocationTracker_CurrentLocationFailed;
                moCurrentLocationTaskCompletionSource = null;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Current location Changed", ex.Message, "OK");
                });
            }
        }

        private static void LocationTracker_CurrentLocationFailed(object sender, GpsData e)
        {
            try
            {
                if (moCurrentLocationTaskCompletionSource != null)
                {
                    moCurrentLocationTaskCompletionSource.SetResult(e);
                }
                System.Diagnostics.Debug.WriteLine($"Current location falied: {DateTime.Now}");

                var loLocationTracker = GetLocationTracker();
                loLocationTracker.CurrentLocationUpdated -= LocationTracker_CurrentLocationUpdated;
                loLocationTracker.CurrentLocationFailed -= LocationTracker_CurrentLocationFailed;
                moCurrentLocationTaskCompletionSource = null;
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Current location falied", ex.Message, "OK");
            }
        }

        public static async Task StartListeningAsync()
        {
            try
            {
                var loLocationTracker = GetLocationTracker();

                // make sure location services are enabled and allowed
                if (!await IsLocationServicesAvailableAsync(true))
                {
                    await App.Current.MainPage.DisplayAlert("Start Listening", "Location service not enabled or not authorized.", "OK");
                    return;
                }

                loLocationTracker.TrackedLocationUpdated += LocationTracker_TrackedLocationUpdated;
                loLocationTracker.TrackedLocationFailed += LocationTracker_TrackedLocationFailed;

                loLocationTracker.StartTracking();
                System.Diagnostics.Debug.WriteLine("Started listening");

            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Start Listening", ex.Message, "OK");
                });
            }
        }

        private static void LocationTracker_TrackedLocationUpdated(object sender, GpsData e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Tracked location changed: {DateTime.Now}");

                var ldNow = DateTime.Now;
                if (ldNow >= mdLastReported + ReportInterval)
                {
                    var leGpsDataArrivedEventArgs = new GpsDataArrivedEventArgs()
                    {
                        Data = e
                    };
                    OnGpsDataArrived(leGpsDataArrivedEventArgs);
                    mdLastReported = ldNow;
                }
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Tracked location Changed", ex.Message, "OK");
                });
            }
        }

        private static void LocationTracker_TrackedLocationFailed(object sender, GpsData e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Tracked location failed: {DateTime.Now}");

                var leGpsDataArrivedEventArgs = new GpsDataArrivedEventArgs()
                {
                    Data = e
                };
                OnGpsDataArrived(leGpsDataArrivedEventArgs);
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Tracked location Failed", ex.Message, "OK");
                });
            }
        }

        public static void StopListening()
        {
            try
            {
                var loLocationTracker = GetLocationTracker();
                loLocationTracker.TrackedLocationUpdated -= LocationTracker_TrackedLocationUpdated;
                loLocationTracker.TrackedLocationFailed -= LocationTracker_TrackedLocationFailed;

                loLocationTracker.StopTracking();
                System.Diagnostics.Debug.WriteLine("Stop listening");
            }
            catch (Exception ex)
            {
                App.Current.MainPage.DisplayAlert("Stop Listening", ex.Message, "OK");
            }
        }

        public static async Task<Coordinate> GeocodeAsync(string address)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Get Lat/Lon");
                var loLocationTracker = GetLocationTracker();
                var loCoordinate = await loLocationTracker.GeocodeAsync(address);
                return loCoordinate;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Geocode", ex.Message, "OK");
                });
            }
            return null;
        }

        public static async Task<string> ReverseGeocodeAsync(Coordinate latLng)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Get address");
                var loLocationTracker = GetLocationTracker();
                var lcAddress = await loLocationTracker.ReverseGeocodeAsync(latLng);
                return lcAddress;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    App.Current.MainPage.DisplayAlert("Reverse Geocode", ex.Message, "OK");
                });
            }
            return null;
        }


    }
}
