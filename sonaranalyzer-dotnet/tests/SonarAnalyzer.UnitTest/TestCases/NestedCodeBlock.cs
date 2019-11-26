using System;
using System.IO;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MeaningOfLife
        {
            get
            {
                { // Noncompliant {{Extract this nested code block into a separate method.}}
//              ^
                    return 42;
                }
            }
            set
            {
                { // Noncompliant
//              ^
                    throw new ArgumentOutOfRangeException("Value can only be 42.");
                }
            }
        }
    }
}
