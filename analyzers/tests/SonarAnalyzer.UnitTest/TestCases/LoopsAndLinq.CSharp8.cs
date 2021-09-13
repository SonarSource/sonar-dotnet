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

        public class Point
        {
            public int X { get; set; }
            public int? Y { get; set; }
        }
    }
}
