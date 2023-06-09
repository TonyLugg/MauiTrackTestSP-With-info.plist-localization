using Android.Gms.Location;
using Android.Util;
using MauiTrackTestSP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Platforms.Android
{
    public class LocationTrackingCallback: LocationCallback
    {
        readonly LocationTracker moLocationTracker;

        public LocationTrackingCallback(LocationTracker locationTracker)
        {
            moLocationTracker = locationTracker;
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("FusedLocationProviderSample", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }

        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var loLocation = result.Locations.First();
                Log.Debug("Location Result", $"Latitude: {loLocation.Latitude}, Longitude: {loLocation.Longitude}");
                moLocationTracker.OnTrackedLocationUpdated(new GpsData()
                {
                    Latitude = loLocation.Latitude,
                    Longitude = loLocation.Longitude,
                    Speed = loLocation.Speed
                });
            }
            else
            {
                // No locations to work with.
            }
        }
    }
}
