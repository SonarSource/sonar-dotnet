using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class LineLength
    {
        public LineLength()
        {
            Console.WriteLine("This is OK..."); // with these comments.........................................................
            Console.WriteLine(); // Noncompliant {{Split this 128 characters long line (which is greater than 127 authorized).}}
        }
    }
}
