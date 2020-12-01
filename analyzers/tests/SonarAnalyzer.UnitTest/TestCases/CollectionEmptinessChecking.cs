using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class CollectionEmptinessChecking
    {
        private static bool HasContent1(IEnumerable<string> l)
        {
            return l.Count() > 0; // Noncompliant {{Use '.Any()' to test whether this 'IEnumerable<string>' is empty or not.}}
//                   ^^^^^
        }
        private static bool HasContent1b(IEnumerable<string> l)
        {
            return 0 < l.Count(); // Noncompliant
        }
        private static bool HasContent2(List<string> l)
        {
            return Enumerable.Count(l) >= 0x1; // Noncompliant
//                            ^^^^^
        }
        private static bool HasContent2b(List<string> l)
        {
            return 1UL <= Enumerable.Count(l); // Noncompliant // Error [CS0034]
        }
        private static bool HasContent3(List<string> l)
        {
            return l.Any();
        }
        private static bool IsEmpty1(List<string> l)
        {
            return l.Count() == 0; // Noncompliant
        }
        private static bool IsEmpty2(List<string> l)
        {
            return l.Count() <= 0; // Noncompliant
        }
        private static bool IsEmpty2b(List<string> l)
        {
            return 0 >= l.Count(); // Noncompliant
        }
        private static bool IsEmpty3(List<string> l)
        {
            return !l.Any();
        }
        private static bool IsEmpty4(List<string> l)
        {
            return l.Count() < 1; // Noncompliant
        }
        private static bool IsEmpty4b(List<string> l)
        {
            return 1 > l.Count(); // Noncompliant
        }
    }
}
