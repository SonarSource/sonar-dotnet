using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class CollectionEmptinessChecking
    {
        private static bool HasContent1(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        private static bool HasContent1b(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        private static bool HasContent2(List<string> l)
        {
            return Enumerable.Any(l); // Fixed
        }
        private static bool HasContent2b(List<string> l)
        {
            return Enumerable.Any(l); // Fixed
        }
        private static bool HasContent3(List<string> l)
        {
            return l.Any();
        }
        private static bool IsNotEmpty1(List<string> l)
        {
            return l.Any(); // Fixed
        }
        private static bool IsNotEmpty2(List<string> l)
        {
            return l.Any(); // Fixed
        }
        private static bool IsEmpty1(List<string> l)
        {
            return !l.Any(); // Fixed
        }
        private static bool IsEmpty2(List<string> l)
        {
            return !l.Any(); // Fixed
        }
        private static bool IsEmpty2b(List<string> l)
        {
            return !l.Any(); // Fixed
        }
        private static bool IsEmpty3(List<string> l)
        {
            return !l.Any();
        }
        private static bool IsEmpty4(List<string> l)
        {
            return !l.Any(); // Fixed
        }
        private static bool IsEmpty4b(List<string> l)
        {
            return !l.Any(); // Fixed
        }
        private static bool HasContentWithCondition(List<int> numbers)
        {
            return numbers.Any(n => n % 2 == 0); // Fixed
        }
        private static bool IsEmptyWithCondition(List<int> numbers)
        {
            return !numbers.Any(n => n % 2 == 0); // Fixed
        }
    }
}
