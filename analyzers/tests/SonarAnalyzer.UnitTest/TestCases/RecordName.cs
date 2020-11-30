namespace t1
{
    record FSM // FN
    {
    }
}
namespace t4
{
    record AbcDEFgh { } // Compliant
    record Ab4DEFgh { } // Compliant
    record Ab4DEFGh { } // FN

    record _AbABaa { } // FN

    record 你好 { } // Compliant
}

namespace TestSuffixes
{
    record IEnumerableExtensionsTest { } // FN, records are not test classes
    record IEnumerableExtensionsTests { } // FN, records are not test classes
}
