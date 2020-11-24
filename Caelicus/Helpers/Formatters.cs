using System;

namespace BlazorApp.Helpers
{
    public class Formatters
    {
        public static string FormatSecondsToPrettyString(double seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"dd\.hh\:mm\:ss");
        }
    }
}
