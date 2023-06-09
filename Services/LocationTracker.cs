using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP.Services
{
    public partial class LocationTracker
    {
        public event EventHandler<GpsData> CurrentLocationUpdated;
        public event EventHandler<GpsData> CurrentLocationFailed;
        public event EventHandler<GpsData> TrackedLocationUpdated;
        public event EventHandler<GpsData> TrackedLocationFailed;
        public partial void GetCurrentLocation();
        public partial void StartTracking();
        public partial void StopTracking();
        public partial Task<Coordinate> GeocodeAsync(string address);
        public partial Task<string> ReverseGeocodeAsync(Coordinate latLng);

    }
}
