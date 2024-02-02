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
            return l.Any(); // Fixed
        }
        private static bool HasContent2b(List<string> l)
        {
            return l.Any();    // Fixed
                               // Error@-1 [CS0034]
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
        public static bool WithReferencedCondition(int[] n)
        {
            return !n.Any(Include); // Fixed
        }
        static bool Include(int n)
        {
            return n == 17;
        }
    }

    public class NotAsExtension
    {
        bool IsEmpty(int[] n)
        {
            return !n.Any(); // Fixed
        }
        bool HasAny(int[] n)
        {
            return n.Any(); // Fixed
        }
        bool RightExpression(int[] n)
        {
            return n.Any(); // Fixed
        }
        bool WithCondition(int[] n)
        {
            return !n.Any(x => x != 2); // Fixed
        }
        bool NotAnIdentifier(string str)
        {
            return str.Trim().Any(); // Fixed
        }
    }

    public class Complex
    {
        bool Nested(string[] words)
        {
            return words.Any(w => !w.Any(ch => ch == 'Q'));
        }
        bool Composed(int[] a, int[] b, int[] c)
        {
            return a.Any() && !b.Any() && c.Any();
        }
    }

    public class Compliant
    {
        bool Any(List<string> list)
        {
            return list.Any();
        }
        bool NotAny(List<string> list)
        {
            return !list.Any();
        }
        bool NotAnExtension(Compliant model)
        {
            return model.Count() == 0;
        }
        bool NotAMethod(int value)
        {
            return value != 0;
        }
        bool SizeDepedent(int[] numbers)
        {
            return numbers.Count() != 2
                || numbers.Count(n => n % 2 == 1) == 3
                || 1 != numbers.Count()
                || 1 == numbers.Count(n => n % 2 == 0)
                || Enumerable.Count(numbers) > 1
                || 42 < Enumerable.Count(numbers);
        }
        bool Undefined(object model)
        {
            return model.Count(); // Error[CS1061]
        }

        int Count() { return 42; }
    }
}
