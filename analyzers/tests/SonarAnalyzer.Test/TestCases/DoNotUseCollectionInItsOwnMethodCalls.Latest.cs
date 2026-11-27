using System.Collections.Generic;
using System.Linq;

public class SpreadOfSelf
{
    public void Method(List<int> list, HashSet<int> set)
    {
        list.Except([..list]);                    // Noncompliant NET-4906
        // Secondary@-1
        list.Intersect([..list]);                 // Noncompliant NET-4906
        // Secondary@-1
        list.SequenceEqual([..list]);             // Noncompliant NET-4906
        // Secondary@-1
        list.Union([..list]);                     // Noncompliant NET-4906
        // Secondary@-1
        Enumerable.SequenceEqual(list, [..list]); // Noncompliant NET-4906
        // Secondary@-1
        set.ExceptWith([..set]);                  // Noncompliant NET-4906
        // Secondary@-1
        set.IntersectWith([..set]);               // Noncompliant NET-4906
        // Secondary@-1
        set.IsProperSubsetOf([..set]);            // Noncompliant NET-4906
        // Secondary@-1
        set.IsProperSupersetOf([..set]);          // Noncompliant NET-4906
        // Secondary@-1
        set.IsSubsetOf([..set]);                  // Noncompliant NET-4906
        // Secondary@-1
        set.IsSupersetOf([..set]);                // Noncompliant NET-4906
        // Secondary@-1
        set.Overlaps([..set]);                    // Noncompliant NET-4906
        // Secondary@-1
        set.SetEquals([..set]);                   // Noncompliant NET-4906
        // Secondary@-1
        set.SymmetricExceptWith([..set]);         // Noncompliant NET-4906
        // Secondary@-1
        set.UnionWith([..set]);                   // Noncompliant NET-4906
        // Secondary@-1
    }

    public void SpreadOnTheLeft(List<int> list)
    {
        Enumerable.Except([..list], [..list]);    // Noncompliant
        // Secondary@-1
        Enumerable.SequenceEqual([..list], list); // Noncompliant
        // Secondary@-1
        Enumerable.Union([..list], list);         // Noncompliant
        // Secondary@-1
    }

    public void NotASpreadOfSelf(List<int> list, List<int> other, HashSet<int> set)
    {
        list.Except([..other]);
        list.Union([..list, 1]);
        set.SetEquals([1, ..set]);
    }
}
