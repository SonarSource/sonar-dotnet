namespace t1
{
    record struct FSM // Noncompliant {{Rename record struct 'FSM' to match pascal case naming rules, consider using 'Fsm'.}}
    //            ^^^
    {
    }
    record struct FSM2(string Param); // Noncompliant
}
namespace t4
{
    record struct AbcDEFgh { } // Compliant
    record struct Ab4DEFgh { } // Compliant
    record struct Ab4DEFGh { } // Noncompliant

    record struct _AbABaa { }  // Noncompliant

    record struct 你好 { }      // Compliant

    record struct AbcDEFgh2(string Param); // Compliant
    record struct Ab4DEFgh2(string Param); // Compliant
    record struct Ab4DEFGh2(string Param); // Noncompliant

    record struct _AbABaa2(string Param);  // Noncompliant

    record struct 你好2(string Param);      // Compliant
}

namespace TestSuffixes
{
    record struct IEnumerableExtensionsTest { }              // Noncompliant
    record struct IEnumerableExtensionsTests { }             // Noncompliant

    record struct IEnumerableExtensionsTest2(string Param);  // Noncompliant
    record struct IEnumerableExtensionsTests2(string Param); // Noncompliant
}
