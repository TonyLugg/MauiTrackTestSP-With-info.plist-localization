using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiTrackTestSP
{
    public class GpsData
    {
        public class ErrorInfo
        {
            public int ID;
            public string Message;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public double KmPerHour
        {
            get
            {
                return Speed * 3.6;
            }
        }
        public ErrorInfo Error { get; set; }

        public override string ToString()
        {
            return $"{Latitude},{Longitude} at {KmPerHour:0} kph";
        }

    }
}
