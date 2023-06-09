using CoreLocation;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Services
{
    // iOS - location tracker
    public partial class LocationTracker
    {
        CLLocationManager moTrackingLocationManager;    // constant movement tracking
        CLLocationManager moCurrentLocationManager;     // single use to request current location

        public LocationTracker()
        {
            moTrackingLocationManager = new CLLocationManager();
            moTrackingLocationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            moTrackingLocationManager.PausesLocationUpdatesAutomatically = false;
            moTrackingLocationManager.ActivityType = CLActivityType.Other;
            moTrackingLocationManager.AllowsBackgroundLocationUpdates = true;
            moTrackingLocationManager.RequestAlwaysAuthorization();
            moTrackingLocationManager.LocationsUpdated += moTrackingLocationManager_LocationsUpdated;
            moTrackingLocationManager.Failed += moTrackingLocationManager_Failed;

            moCurrentLocationManager = new CLLocationManager();
            //moCurrentLocationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            moCurrentLocationManager.LocationsUpdated += moCurrentLocationManager_LocationsUpdated;
            moCurrentLocationManager.Failed += moCurrentLocationManager_Failed;
        }

        private void moTrackingLocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            // get the latest location (current)
            var liLocationCount = e.Locations.Length;
            CLLocation loLocation = e.Locations[liLocationCount - 1];

            // fire event
            TrackedLocationUpdated?.Invoke(this, new GpsData()
            {
                Latitude = loLocation.Coordinate.Latitude,
                Longitude = loLocation.Coordinate.Longitude,
                Speed = loLocation.Speed,
                Heading = loLocation.Course
            });

        }

        private void moTrackingLocationManager_Failed(object sender, NSErrorEventArgs e)
        {
            // fire event
            string lcMessage = "Unknown error";
            if ((CLError)(int)e.Error.Code == CLError.LocationUnknown)
            {
                lcMessage = "Location unknown";
            }
            if ((CLError)(int)e.Error.Code == CLError.Denied)
            {
                lcMessage = "Location access denied by user";
            }
            if ((CLError)(int)e.Error.Code == CLError.Network)
            {
                lcMessage = "Network issue";
            }
            if ((CLError)(int)e.Error.Code == CLError.HeadingFailure)
            {
                lcMessage = "Heading failure";
            }
            TrackedLocationFailed?.Invoke(this, new GpsData()
            {
                Error = new GpsData.ErrorInfo()
                {
                    ID = (int)e.Error.Code,
                    Message = lcMessage
                }
            });
        }

        private void moCurrentLocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            // get the latest location (current)
            var liLocationCount = e.Locations.Length;
            CLLocation loLocation = e.Locations[liLocationCount - 1];

            // fire event
            CurrentLocationUpdated?.Invoke(this, new GpsData()
            {
                Latitude = loLocation.Coordinate.Latitude,
                Longitude = loLocation.Coordinate.Longitude,
                Speed = loLocation.Speed
            });
        }

        private void moCurrentLocationManager_Failed(object sender, NSErrorEventArgs e)
        {
            // fire event
            string lcMessage = "Unknown error";
            if ((CLError)(int)e.Error.Code == CLError.LocationUnknown)
            {
                lcMessage = "Location unknown";
            }
            if ((CLError)(int)e.Error.Code == CLError.Denied)
            {
                lcMessage = "Location access denied by user";
            }
            if ((CLError)(int)e.Error.Code == CLError.Network)
            {
                lcMessage = "Network issue";
            }
            if ((CLError)(int)e.Error.Code == CLError.HeadingFailure)
            {
                lcMessage = "Heading failure";
            }
            CurrentLocationFailed?.Invoke(this, new GpsData()
            {
                Error = new GpsData.ErrorInfo()
                {
                    ID = (int)e.Error.Code,
                    Message = lcMessage
                }
            });
        }

        public partial void GetCurrentLocation()
        {
            moCurrentLocationManager.RequestLocation();
        }

        public partial void StartTracking()
        {
            moTrackingLocationManager.StartUpdatingLocation();
        }

        public partial void StopTracking()
        {
            moTrackingLocationManager.StopUpdatingLocation();
        }

        public async partial Task<Coordinate> GeocodeAsync(string address)
        {
            using (var loGeocoder = new CLGeocoder())
            {
                var lcolPlacemarks = await loGeocoder.GeocodeAddressAsync(address);
                if (lcolPlacemarks != null)
                {
                    var loPlacemark = lcolPlacemarks[0];
                    return new Coordinate(loPlacemark.Location.Coordinate.Latitude, loPlacemark.Location.Coordinate.Longitude);
                }
                return null;
            }
        }

        public async partial Task<string> ReverseGeocodeAsync(Coordinate latLng)
        {
            using (var loGeocoder = new CLGeocoder())
            {
                var lcolPlacemarks = await loGeocoder.ReverseGeocodeLocationAsync(new CLLocation(latLng.Latitude, latLng.Longitude));
                if (lcolPlacemarks != null)
                {
                    var loPlacemark = lcolPlacemarks[0];
                    var lcAddress = "";
                    if (loPlacemark.SubThoroughfare != null)
                    {
                        lcAddress += loPlacemark.SubThoroughfare;
                    }
                    if (loPlacemark.Thoroughfare != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += " ";
                        }
                        lcAddress += loPlacemark.Thoroughfare;
                    }
                    if (loPlacemark.Locality != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += ", ";
                        }
                        lcAddress += loPlacemark.Locality;
                    }
                    if (loPlacemark.AdministrativeArea != null)
                    {
                        if (lcAddress.Length > 0)
                        {
                            lcAddress += ", ";
                        }
                        lcAddress += loPlacemark.AdministrativeArea;
                    }
                    return lcAddress;
                }
                return null;
            }
        }

    }
}
