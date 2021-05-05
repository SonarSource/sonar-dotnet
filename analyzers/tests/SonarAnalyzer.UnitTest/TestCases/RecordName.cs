namespace t1
{
    record FSM // Noncompliant
    {
    }
    record FSM2(string Param); // Noncompliant
}
namespace t4
{
    record AbcDEFgh { } // Compliant
    record Ab4DEFgh { } // Compliant
    record Ab4DEFGh { } // Noncompliant

    record _AbABaa { }  // Noncompliant

    record 你好 { }      // Compliant

    record AbcDEFgh2(string Param); // Compliant
    record Ab4DEFgh2(string Param); // Compliant
    record Ab4DEFGh2(string Param); // Noncompliant

    record _AbABaa2(string Param);  // Noncompliant

    record 你好2(string Param);      // Compliant
}

namespace TestSuffixes
{
    record IEnumerableExtensionsTest { }              // Noncompliant
    record IEnumerableExtensionsTests { }             // Noncompliant

    record IEnumerableExtensionsTest2(string Param);  // Noncompliant
    record IEnumerableExtensionsTests2(string Param); // Noncompliant
}
