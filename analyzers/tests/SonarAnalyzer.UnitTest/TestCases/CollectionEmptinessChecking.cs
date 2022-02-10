using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class CollectionEmptinessChecking
    {
        private static bool HasContent1(IEnumerable<string> l)
        {
            return l.Count() > 0; // Noncompliant {{Use '.Any()' to test whether this 'IEnumerable<string>' is empty or not.}}
            //       ^^^^^
        }
        private static bool HasContent1b(IEnumerable<string> l)
        {
            return 0 < l.Count(); // Noncompliant
        }
        private static bool HasContent2(List<string> l)
        {
            return Enumerable.Count(l) >= 0x1; // Noncompliant
            //                ^^^^^
        }
        private static bool HasContent2b(List<string> l)
        {
            return 1UL <= Enumerable.Count(l); // Noncompliant // Error[CS0034]
        }
        private static bool IsNotEmpty1(List<string> l)
        {
            return l.Count() != 0; // Noncompliant
        }
        private static bool IsNotEmpty2(List<string> l)
        {
            return 0 != l.Count(); // Noncompliant
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
        private static bool IsEmpty4(List<string> l)
        {
            return l.Count() < 1; // Noncompliant
        }
        private static bool IsEmpty4b(List<string> l)
        {
            return 1 > l.Count(); // Noncompliant
        }
        private static bool HasContentWithCondition(List<int> numbers)
        {
            return numbers.Count(n => n % 2 == 0) > 0; // Noncompliant
        }
        private static bool IsEmptyWithCondition(List<int> numbers)
        {
            return numbers.Count(n => n % 2 == 0) == 0; // Noncompliant
        }
    }

    public class Complex
    {
        bool Nested(string[] words)
        {
            return words.Count(w => w.Count(ch => ch == 'Q') == 0) > 0;
            //           ^^^^^                  {{Use '.Any()' to test whether this 'IEnumerable<string>' is empty or not.}}
            //                        ^^^^^ @-1 {{Use '.Any()' to test whether this 'IEnumerable<char>' is empty or not.}}
        }
        bool Composed(int[] a, int[] b, int[] c)
        {
            return a.Count() > 0 && b.Count() == 0 && c.Count() > 0;
            //       ^^^^^                                        {{Use '.Any()' to test whether this 'IEnumerable<int>' is empty or not.}}
            //                        ^^^^^                   @-1 {{Use '.Any()' to test whether this 'IEnumerable<int>' is empty or not.}}
            //                                          ^^^^^ @-2 {{Use '.Any()' to test whether this 'IEnumerable<int>' is empty or not.}}
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
