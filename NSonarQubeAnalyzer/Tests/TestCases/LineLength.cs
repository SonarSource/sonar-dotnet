namespace Tests.Diagnostics
{
    using System.Collections.Generic;

    public class LineLength
    {
        public LineLength()
        {
            Console.WriteLine("This is OK....);
            Console.WriteLine(); // Noncompliant
        }
    }
}
