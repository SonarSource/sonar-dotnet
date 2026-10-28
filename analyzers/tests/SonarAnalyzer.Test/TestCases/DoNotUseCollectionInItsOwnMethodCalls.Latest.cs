using System.Collections.Generic;
using System.Linq;

public class SpreadOfSelf
{
    public void Method(List<int> list, HashSet<int> set)
    {
        list.Except([..list]);                    // FN NET-4906
        list.Intersect([..list]);                 // FN NET-4906
        list.SequenceEqual([..list]);             // FN NET-4906
        list.Union([..list]);                     // FN NET-4906
        Enumerable.SequenceEqual(list, [..list]); // FN NET-4906
        set.ExceptWith([..set]);                  // FN NET-4906
        set.IntersectWith([..set]);               // FN NET-4906
        set.IsProperSubsetOf([..set]);            // FN NET-4906
        set.IsProperSupersetOf([..set]);          // FN NET-4906
        set.IsSubsetOf([..set]);                  // FN NET-4906
        set.IsSupersetOf([..set]);                // FN NET-4906
        set.Overlaps([..set]);                    // FN NET-4906
        set.SetEquals([..set]);                   // FN NET-4906
        set.SymmetricExceptWith([..set]);         // FN NET-4906
        set.UnionWith([..set]);                   // FN NET-4906
    }
}
