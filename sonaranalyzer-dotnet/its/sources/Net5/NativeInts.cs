using System;

namespace Net5
{
    public class NativeInts
    {
        public void Method()
        {
            nint i = -1;
            nuint i2 = 2;
            Console.WriteLine($"Type of {i} is {typeof(nint)}."); // System.IntPtr.
            Console.WriteLine($"Type of {i2} is {typeof(nuint)}."); // System.UIntPtr
        }
    }
}
