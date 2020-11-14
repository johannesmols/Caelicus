using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorLeaflet.Models;
using GeoCoordinatePortable;

namespace Caelicus.Helpers
{
    public static class GeographicalHelpers
    {
        private const int EarthRadiusKm = 6371;
        private const double RadianCoeff = Math.PI / 180;
        private const double DegreesCoeff = 180 / Math.PI;

        public static double CalculateGeographicalDistanceInMeters(Tuple<double, double> start, Tuple<double, double> end)
        {
            var pos1 = new GeoCoordinate(start.Item1, start.Item2);
            var pos2 = new GeoCoordinate(end.Item1, end.Item2);
            return pos1.GetDistanceTo(pos2);
        }

        // TODO: This is probably pretty inaccurate over long distances but since it is only used to display vehicles on the map, it's fine for now
        public static Tuple<double, double> CalculatePointInBetweenTwoPoints(Tuple<double, double> start, Tuple<double, double> end, double progress)
        {
            if (progress >= 1d)
                progress = 1d;

            var difference = Tuple.Create(start.Item1 - end.Item1, start.Item2 - end.Item2);
            var middlePoint = Tuple.Create(start.Item1 - (difference.Item1 * progress), start.Item2 - (difference.Item2 * progress));

            return middlePoint;
        }


        // https://stackoverflow.com/a/58063187/2102106

        /// <summary>
        /// Get distance in km between two GPS points.
        /// </summary>
        public static double GetDistance(Tuple<double, double> from, Tuple<double, double> to)
        {
            var differenceLatitudeRadian = (to.Item1 - from.Item1) * RadianCoeff;
            var differenceLongitudeRadian = (to.Item2 - from.Item2) * RadianCoeff;

            var startLatitudeRadian = from.Item1 * RadianCoeff;
            var endLatitudeRadian = to.Item1 * RadianCoeff;

            var a = Math.Sin(differenceLatitudeRadian / 2) * Math.Sin(differenceLatitudeRadian / 2) +
                    Math.Sin(differenceLongitudeRadian / 2) * Math.Sin(differenceLongitudeRadian / 2) *
                    Math.Cos(startLatitudeRadian) * Math.Cos(endLatitudeRadian);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var distance = c * EarthRadiusKm;
            return distance;
        }

        /// <summary>
        /// Get GPS point on a way from A to B based on A coordinates, bearing and distance.
        /// </summary>
        public static Tuple<double, double> GetIntermediateLatLng(Tuple<double, double> from, double bearing, double distance)
        {
            // https://www.movable-type.co.uk/scripts/latlong.html

            if (from == null)
            {
                return null;
            }

            // Find intermediate LatLng based on to, bearing and distance
            return FindDestinationForGivenStartPointAndBearing(from, bearing, distance);
        }

        public static double FindInitialBearing(Tuple<double, double> from, Tuple<double, double> to)
        {
            if (from == null || to == null)
            {
                return 0;
            }

            var fromLatRadian = from.Item1 * RadianCoeff;
            var toLatRadian = to.Item1 * RadianCoeff;
            var longitudeDeltaRadian = (to.Item2 - from.Item2) * RadianCoeff;

            var x = Math.Cos(fromLatRadian) * Math.Sin(toLatRadian)
                - Math.Sin(fromLatRadian) * Math.Cos(toLatRadian) * Math.Cos(longitudeDeltaRadian);
            var y = Math.Sin(longitudeDeltaRadian) * Math.Cos(toLatRadian);
            var t = Math.Atan2(y, x);

            var bearing = t * DegreesCoeff;

            return Wrap360(bearing);
        }

        public static Tuple<double, double> FindDestinationForGivenStartPointAndBearing(Tuple<double, double> from, double bearing, double distance)
        {
            if (from == null)
            {
                return null;
            }

            // Angular distance in radians
            var angular = distance / EarthRadiusKm;
            var bearingRadian = bearing * RadianCoeff;

            var latitudeRadian = from.Item1 * RadianCoeff;
            var longitudeRadian = from.Item2 * RadianCoeff;

            var destLatSine = Math.Sin(latitudeRadian) * Math.Cos(angular)
                + Math.Cos(latitudeRadian) * Math.Sin(angular) * Math.Cos(bearingRadian);
            var destLatitudeRadian = Math.Asin(destLatSine);

            var y = Math.Sin(bearingRadian) * Math.Sin(angular) * Math.Cos(latitudeRadian);
            var x = Math.Cos(angular) - Math.Sin(latitudeRadian) * destLatSine;
            var destLongitudeRadian = longitudeRadian + Math.Atan2(y, x);

            var destLatitude = destLatitudeRadian * DegreesCoeff;
            var destLongitude = destLongitudeRadian * DegreesCoeff;

            return Tuple.Create(Math.Round(Wrap90(destLatitude), 6), Math.Round(Wrap180(destLongitude), 6));
        }

        public static double Wrap360(double degrees)
        {
            // Avoid rounding due to arithmetic ops if within range
            if (0 <= degrees && degrees < 360)
            {
                return degrees;
            }

            // Sawtooth wave p:360, a:360
            return (degrees % 360 + 360) % 360;
        }

        public static double Wrap180(double degrees)
        {
            // Avoid rounding due to arithmetic ops if within range
            if (-180 < degrees && degrees <= 180)
            {
                return degrees;
            }

            // Sawtooth wave p:180, a:±180
            return (degrees + 540) % 360 - 180;
        }

        public static double Wrap90(double degrees)
        {
            // Avoid rounding due to arithmetic ops if within range
            if (-90 <= degrees && degrees <= 90)
            {
                return degrees;
            }

            // Triangle wave p:360 a:±90 TODO: fix e.g. -315°
            return Math.Abs((degrees % 360 + 270) % 360 - 180) - 90;
        }
    }
}
