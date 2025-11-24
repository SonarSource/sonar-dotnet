using System;
using System.Collections.Generic;

namespace PartialMembers
{
    public partial class DescendantClass : BaseClass
    {
        private partial int Property_01 { get; } // Noncompliant
        //                  ^^^^^^^^^^^
        private partial int this[int index] { get; } // Noncompliant
        //                  ^^^^
        private partial event EventHandler SomeEvent;
    }
}
