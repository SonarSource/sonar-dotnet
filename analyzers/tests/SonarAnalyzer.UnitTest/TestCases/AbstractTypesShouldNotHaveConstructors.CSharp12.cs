using System;
using System.Collections.Generic;

namespace CSharp12
{
    abstract class PrimaryConstructor(int i) // FN - Primary constructor is public by default
    {
        public int I { get; set; } = i;
    }
}
