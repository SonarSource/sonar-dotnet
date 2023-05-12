using System.Linq;
using System.Collections.Generic;

namespace Net7.features
{
    public static class SetPropertiesInsteadOfLinqMethods
    {
        static int Method()
        {
            var set = new SortedSet<int>();
            return set.Min();
        }
    }
}
