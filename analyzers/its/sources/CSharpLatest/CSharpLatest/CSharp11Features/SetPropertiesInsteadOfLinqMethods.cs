using System.Linq;
using System.Collections.Generic;

namespace CSharpLatest.CSharp11Features;

public static class SetPropertiesInsteadOfLinqMethods
{
    static int Method()
    {
        var set = new SortedSet<int>();
        return set.Min();
    }
}
