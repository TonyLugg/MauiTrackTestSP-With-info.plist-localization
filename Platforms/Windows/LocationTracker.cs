using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace MauiTrackTestSP.Services
{
    // Windows
    public partial class LocationTracker
    {
        const string MapKey = "QhsJLhIyHl6VNpk1cY1p~_vMCueKqGeFqmrmffkJ0NQ~AgA1ttoP9zFzBAjdpOs3BezO24HF7RGWxCTwBd_-SpqE8lP6FM0q-CbFGOdEh142";

        public async partial void GetCurrentLocation()
        {
            var loGeolocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High
            };
            switch (loGeolocator.LocationStatus)
            {
                case PositionStatus.Disabled:
                case PositionStatus.NotAvailable:
                    CurrentLocationFailed?.Invoke(this, new GpsData()
                    {
                        Error = new GpsData.ErrorInfo()
                        {
                            ID = (int)0,
                            Message = "Location services are not enabled on device."
                        }
                    });
                    return;
            }
            var loLocation = await loGeolocator.GetGeopositionAsync();
            CurrentLocationUpdated?.Invoke(this, new GpsData()
            {
                Latitude = loLocation.Coordinate.Latitude,
                Longitude = loLocation.Coordinate.Longitude
            });
        }

        public partial void StartTracking()
        {
            return;
        }

        public partial void StopTracking()
        {
            return;
        }

        public async partial Task<Coordinate> GeocodeAsync(string address)
        {
            MapService.ServiceToken = MapKey;
            var laAddresses = await MapLocationFinder.FindLocationsAsync(address, null, 1);
            if (laAddresses?.Locations?.Any() ?? false)
            {
                var loAddress = laAddresses.Locations[0];
                return new Coordinate(loAddress.Point.Position.Latitude, loAddress.Point.Position.Longitude);
            }
            return null;
        }

        public async partial Task<string> ReverseGeocodeAsync(Coordinate latLng)
        {
            MapService.ServiceToken = MapKey;
            var loGeopoint = new Geopoint(new BasicGeoposition { Latitude = latLng.Latitude, Longitude = latLng.Longitude });
            var laAddresses = await MapLocationFinder.FindLocationsAtAsync(loGeopoint);
            if (laAddresses?.Locations?.Any() ?? false)
            {
                var loAddress = laAddresses.Locations[0];
                var lcAddress = "";
                if (loAddress.Address.StreetNumber != null)
                {
                    lcAddress += loAddress.Address.StreetNumber;
                }
                if (loAddress.Address.Street != null)
                {
                    if (lcAddress.Length > 0)
                    {
                        lcAddress += " ";
                    }
                    lcAddress += loAddress.Address.Street;
                }
                if (loAddress.Address.Town != null)
                {
                    if (lcAddress.Length > 0)
                    {
                        lcAddress += ", ";
                    }
                    lcAddress += loAddress.Address.Town;
                }
                if (loAddress.Address.Region != null)
                {
                    if (lcAddress.Length > 0)
                    {
                        lcAddress += ", ";
                    }
                    lcAddress += loAddress.Address.Region;
                }
                return lcAddress;
            }
            return null;
        }

    }
}
