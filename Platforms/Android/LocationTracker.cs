using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.Util;
using MauiTrackTestSP.Platforms.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Media.MicrophoneInfo;

namespace MauiTrackTestSP.Services
{
    // Android - location tracker
    public partial class LocationTracker
    {
        FusedLocationProviderClient moFusedLocationClient;
        public static MainActivity MainActivityInstance { get; set; }

        public void OnTrackedLocationUpdated(GpsData e)
        {
            TrackedLocationUpdated?.Invoke(this, e);
        }

        public LocationTracker()
        {
            moFusedLocationClient = LocationServices.GetFusedLocationProviderClient(MainActivityInstance);
            LocationTrackerService.LocationTrackerInstance = this;
        }

        public async partial void GetCurrentLocation()
        {
            var loLocation = await moFusedLocationClient.GetLastLocationAsync();
            Console.WriteLine($"GetCurrentLocation result: {loLocation.Latitude}, {loLocation.Longitude}");

            // fire event
            CurrentLocationUpdated?.Invoke(this, new GpsData()
            {
                Latitude = loLocation.Latitude,
                Longitude = loLocation.Longitude,
                Speed = loLocation.Speed
            });

        }

        public partial void StartTracking()
        {
            //var loLocationRequest = new LocationRequest()
            //    .SetPriority(LocationRequest.PriorityHighAccuracy)
            //    .SetInterval(15 * 1000)
            //    .SetFastestInterval(5 * 1000);
            //if (moLocationTrackingCallback == null)
            //{
            //    moLocationTrackingCallback = new LocationTrackingCallback(this);
            //}
            //await moFusedLocationClient.RequestLocationUpdatesAsync(loLocationRequest, moLocationTrackingCallback);

            var loStartServiceIntent = new Intent(MainActivityInstance, typeof(LocationTrackerService));
            loStartServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
            MainActivityInstance.StartForegroundService(loStartServiceIntent);

            Log.Info("LocationTracker", "Requested service be started.");
        }

        public partial void StopTracking()
        {
            //if (moLocationTrackingCallback != null)
            //{
            //    moFusedLocationClient.RemoveLocationUpdatesAsync(moLocationTrackingCallback);
            //    moLocationTrackingCallback = null;
            //}

            var loStopServiceIntent = new Intent(MainActivityInstance, typeof(LocationTrackerService));
            loStopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);
            MainActivityInstance.StopService(loStopServiceIntent);
        }

        public async partial Task<Coordinate> GeocodeAsync(string address)
        {
            using (var loGeocoder = new Geocoder(MainActivityInstance))
            {
                var lcolAddresses = await loGeocoder.GetFromLocationNameAsync(address, 10);
                if (lcolAddresses?.Any() ?? false)
                {
                    var loAddress = lcolAddresses[0];
                    return new Coordinate(loAddress.Latitude, loAddress.Longitude);
                }
                return null;
            }
        }

        public async partial Task<string> ReverseGeocodeAsync(Coordinate latLng)
        {
            using (var loGeocoder = new Geocoder(MainActivityInstance))
            {
                var lcolAddresses = await loGeocoder.GetFromLocationAsync(latLng.Latitude, latLng.Longitude, 10);
                if (lcolAddresses?.Any() ?? false)
                {
                    var loAddress = lcolAddresses[0];
                    var lcAddress = "";
                    if (loAddress.SubThoroughfare != null)
                    {
                        lcAddress += loAddress.SubThoroughfare;
                    }
                    if (loAddress.Thoroughfare != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += " ";
                        }
                        lcAddress += loAddress.Thoroughfare;
                    }
                    if (loAddress.Locality != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += ", ";
                        }
                        lcAddress += loAddress.Locality;
                    }
                    if (loAddress.AdminArea != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += ", ";
                        }
                        lcAddress += loAddress.AdminArea;
                    }
                    return lcAddress;
                }
                return null;
            }
        }


    }
}
