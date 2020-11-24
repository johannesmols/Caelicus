using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Helpers
{
    public class Formatters
    {
        public static string FormatSecondsToPrettyString(double seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"dd\.hh\:mm\:ss");
        }
    }
}
