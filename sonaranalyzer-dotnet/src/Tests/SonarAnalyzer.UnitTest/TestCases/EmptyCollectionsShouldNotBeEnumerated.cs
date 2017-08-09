using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program 
    {
        private List<int> l;

        public void Main()
        {
            var c = new List<int>();
            c.Remove(1); // Noncompliant
            c.Add(1); // Compliant
            c.Remove(1); // Compliant
        }
    }
}
