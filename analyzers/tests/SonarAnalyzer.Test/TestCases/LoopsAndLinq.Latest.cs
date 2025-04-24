using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace CSharpEight
{
    public class S3267
    {
        public void ForEach_PropertySet_Compliant(ICollection<Point> collection)
        {
            foreach (var point in collection) // Compliant - Selecting `Y` and setting it's value will not work in this case.
            {
                point.Y ??= 3;
                Console.WriteLine(point.Y);
            }
        }

        public void ForEach_UsingDynamic_Compliant(ICollection<Point> collection)
        {
            var sum = 0;
            foreach (dynamic point in collection) // Compliant - with dynamic we cannot know for sure
            {
                sum = point.X + point.X + 3;
            }
        }

        public class Point
        {
            public int X { get; set; }
            public int? Y { get; set; }
        }

        void IsPatterns(List<string> strings, List<Tuple<string, int>> tuples)
        {
            const string target = "42";

            foreach (var s in strings) // Noncompliant
            {
                if (s is target) // Secondary
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var s in strings) // Compliant, do not raise on VarPattern in IsPattern
            {
                if (s is var s2)
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var s in strings) // Compliant, do not raise on SingleVariableDeclaration in IsPattern
            {
                if (s is { Length: 42 } str)
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var t in tuples) // Compliant, do not raise on ParenthesizedVariableDeclaration in IsPattern
            {
                if (t is var (t1, t2))
                {
                    Console.WriteLine("Pattern match successful");
                }
            }
        }
    }
}

namespace CSharpEleven
{
    class IsPatternTests
    {
        void ListPattern(List<int[]> list)
        {
            foreach (int[] array in list) // Noncompliant
            {
                if (array is [1, 2, 3]) // Secondary
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var array in list) // Compliant, do not raise on VarPattern in ListPattern
            {
                if (array is [1, var x, var z])
                {
                    Console.WriteLine("Pattern match successful");
                }

            }

            foreach (var array in list) // Compliant, do not raise on declaration statements in ListPattern
            {
                if (array is [1, ..] local)
                {
                    Console.WriteLine("Pattern match successful");
                }

            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7730
    class Repro_7730
    {
        void SpansAndLogicalPatterns(Span<char> s, ReadOnlySpan<char> ros)
        {
            foreach (var c in s)                   // Noncompliant, FP: iterable but not enumerable, nor queriable
                if (c is ' ') { }                  // Secondary
            foreach (var c in s)                   // Noncompliant, FP
                if (c is not ' ') { }              // Secondary
            foreach (var c in s)                   // Noncompliant, FP
                if (c is ' ' or '\n' or '\r') { }  // Secondary

            foreach (var c in ros)                 // Noncompliant, FP
                if (c is ' ') { }                  // Secondary
            foreach (var c in ros)                 // Noncompliant, FP
                if (c is not ' ') { }              // Secondary
            foreach (var c in ros)                 // Noncompliant, FP
                if (c is ' ' or '\n' or '\r') { }  // Secondary
        }

        void SpansAndLogicalOperators(Span<char> s, ReadOnlySpan<char> ros)
        {
            foreach (var c in s)                             // Compliant: iterable but not enumerable, nor queriable
                if (c == ' ') { }
            foreach (var c in s)                             // Compliant
                if (c != ' ') { }
            foreach (var c in s)                             // Compliant
                if (c == ' ' || c == '\n' || c == '\r') { }

            foreach (var c in ros)                           // Compliant
                if (c == ' ') { }
            foreach (var c in ros)                           // Compliant
                if (c != ' ') { }
            foreach (var c in ros)                           // Compliant
                if (c == ' ' || c == '\n' || c == '\r') { }
        }

        void IterableNotEnumerableAndLogicalPatterns(IterableNotEnumerable s)
        {
            foreach (var c in s)                   // Noncompliant, FP: iterable but not enumerable, nor queriable
                if (c is ' ') { }                  // Secondary
            foreach (var c in s)                   // Noncompliant, FP
                if (c is not ' ') { }              // Secondary
            foreach (var c in s)                   // Noncompliant, FP
                if (c is ' ' or '\n' or '\r') { }  // Secondary
        }

        void EnumerableNotCollectionAndLogicalPatterns()
        {
            foreach (var c in EnumerableNotCollection()) // Noncompliant
                if (c is ' ') { }                        // Secondary
            foreach (var c in EnumerableNotCollection()) // Noncompliant
                if (c is not ' ') { }                    // Secondary
            foreach (var c in EnumerableNotCollection()) // Noncompliant
                if (c is ' ' or '\n' or '\r') { }        // Secondary

            IEnumerable<char> EnumerableNotCollection()
            {
                yield return 'a';
                yield return 'b';
            }
        }

        class IterableNotEnumerable
        {
            public IEnumerator<char> GetEnumerator()
            {
                yield return 'a';
                yield return 'b';
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7776
    class Repro_7776
    {
        class ForEach_WithCustomList
        {
            void Test(CustomList s)
            {
                foreach (var c in s)                   // Noncompliant
                    if (c is ' ') { }                  // Secondary
                foreach (var c in s)                   // Noncompliant
                    if (c is not ' ') { }              // Secondary
                foreach (var c in s)                   // FN, equivalent to c is ' '
                    if (c == ' ') { }
                foreach (var c in s)                   // FN, equivalent to c is not ' '
                    if (c != ' ') { }
            }

            class CustomList : List<char>
            {
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8430
    class Repro_8430
    {
        void Visit(ReadOnlySpan<char> x) { }
        void Visit(MyRefStruct x) { }

        void Test(IEnumerable<ReadOnlyMemory<char>> tokens)
        {
            foreach (ReadOnlyMemory<char> token in tokens) // Compliant
            {
                ReadOnlySpan<char> chars = token.Span;
                Visit(chars);
            }
        }

        public class Data
        {
            public MyRefStruct RefStruct => new MyRefStruct();
        }

        public ref struct MyRefStruct
        {

        }

        void TestRefStruct(IEnumerable<Data> datas)
        {
            foreach (Data data in datas) // Compliant
            {
                MyRefStruct refStruct = data.RefStruct;
                Visit(refStruct);
            }
        }
    }
}

namespace CSharp13
{
    public class NewMethods
    {
        async void WhenEach(List<Task<string>> tasks)
        {
            await foreach (var s in Task.WhenEach(tasks)) // Noncompliant
            {
                if (s.Result is "42")  //Secondary
                {
                    Console.WriteLine(s.Result);
                }
            }
        }

        void IndexTest(List<string> strings)
        {
            foreach (var s in strings.Index()) // Noncompliant
            {
                if (s is (42, "42"))    // Secondary
                {
                    Console.WriteLine(s);
                }
            }
        }
    }
}
