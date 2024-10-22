using System;

namespace CSharp13
{
    internal partial class PartialPropertyClass //Noncompliant
    {
        public partial int PropertyA { get { return 42; } } // Secondary
        private partial int PropertyE { get  { return 42; } }
    }

    //https://sonarsource.atlassian.net/browse/NET-560
    partial class OtherPartialPropertyClass // CompliantFN
    {
        public partial int PropertyA { get { return 42; } } // Compliant FN
        private partial int PropertyE { get { return 42; } }

        public partial void VoidMethod();  // Compliant FN
        partial void AnotherAnotherVoidMethod();

        public partial void DoSomething() { } // Compliant FN
        private partial void DoSomethingElse() { }
    }
}
