using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SonarAnalyzer.UnitTest")]

namespace SonarAnalyzer.UnitTest.TestCases
{
    public class Foo
    {
        internal Foo() { } // Compliant because of InternalsVisibleToAttribute

        internal int MyProperty { get; set; } // Compliant because of InternalsVisibleToAttribute
    }
}
