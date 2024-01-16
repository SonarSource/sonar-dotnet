using System;
using System.Collections.Generic;

namespace CSharp12
{
    abstract class PrimaryConstructor(int i) // Compliant
    {
        public int I { get; set; } = i;
    }
}
