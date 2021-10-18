namespace t1
{
    record struct FSM // FN
    {
    }
    record struct FSM2(string Param); // FN
}
namespace t4
{
    record struct AbcDEFgh { } // Compliant
    record struct Ab4DEFgh { } // Compliant
    record struct Ab4DEFGh { } // FN

    record struct _AbABaa { }  // FN

    record struct 你好 { }      // Compliant

    record struct AbcDEFgh2(string Param); // Compliant
    record struct Ab4DEFgh2(string Param); // Compliant
    record struct Ab4DEFGh2(string Param); // FN

    record struct _AbABaa2(string Param);  // FN

    record struct 你好2(string Param);      // Compliant
}

namespace TestSuffixes
{
    record struct IEnumerableExtensionsTest { }              // FN
    record struct IEnumerableExtensionsTests { }             // FN

    record struct IEnumerableExtensionsTest2(string Param);  // FN
    record struct IEnumerableExtensionsTests2(string Param); // FN
}
