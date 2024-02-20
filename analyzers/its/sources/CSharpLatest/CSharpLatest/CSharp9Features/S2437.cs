namespace CSharpLatest.CSharp9Features;

public class S2437
{
    public void Foo()
    {
        nint result;
        nint bitMask = 0x010F;

        result = bitMask & -1;
    }
}
