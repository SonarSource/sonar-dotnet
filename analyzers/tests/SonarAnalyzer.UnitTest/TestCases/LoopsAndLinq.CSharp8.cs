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
    }
}
