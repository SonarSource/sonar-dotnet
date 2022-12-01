using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Diagnostics
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
