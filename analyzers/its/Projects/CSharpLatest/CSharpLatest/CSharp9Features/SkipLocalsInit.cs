using System.Runtime.CompilerServices;

namespace CSharpLatest.CSharp9Features;

public class SkipLocalsInit
{
    [SkipLocalsInit]
    public void Method()
    {
        int i; // not initialized to 0
    }
}
