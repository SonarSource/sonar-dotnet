using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LineLength
    {
        public LineLength()
        {
            Console.WriteLine("This is OK...");
            Console.WriteLine(); // Noncompliant
        }
    }
}
