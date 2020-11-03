using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoCoordinatePortable;

namespace Caelicus.Helpers
{
    public static class GeographicalHelpers
    {
        public static double CalculateGeographicalDistanceInMeters(Tuple<double, double> start, Tuple<double, double> end)
        {
            var pos1 = new GeoCoordinate(start.Item1, start.Item2);
            var pos2 = new GeoCoordinate(end.Item1, end.Item2);
            return pos1.GetDistanceTo(pos2);
        }
    }
}
