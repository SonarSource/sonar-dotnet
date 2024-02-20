namespace CSharpLatest.CSharp11Features;

internal class UnsignedRightShiftOperator
{
    public void Method()
    {
        int x = -8;
        Console.WriteLine($"Before:    {x,11}, hex: {x,8:x}, binary: {Convert.ToString(x, toBase: 2),32}");

        int y = x >> 2;
        Console.WriteLine($"After  >>: {y,11}, hex: {y,8:x}, binary: {Convert.ToString(y, toBase: 2),32}");

        int z = x >>> 2;
        Console.WriteLine($"After >>>: {z,11}, hex: {z,8:x}, binary: {Convert.ToString(z, toBase: 2).PadLeft(32, '0'),32}");
    }
}
