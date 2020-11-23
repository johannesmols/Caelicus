using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Caelicus.Helpers
{
    public class ListUtilities
    {
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (var i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
