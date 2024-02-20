using System;

namespace CSharpLatest.CSharp9Features;

public class AttributesOnLocalFunctions
{
    public void Method()
    {
        LocalFunction(); // CS0612 'LocalFunction' is obsolete.

        [Obsolete]
        void LocalFunction() { }
    }
}
