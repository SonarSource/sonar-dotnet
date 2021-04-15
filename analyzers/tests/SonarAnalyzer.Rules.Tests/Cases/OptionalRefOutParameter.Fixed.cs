using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class OptionalRefOutParameter
    {
        public void DoStuff(ref int i) // Fixed
        {
            Console.WriteLine(i);
        }
        public void DoStuff2(out int i) // Fixed
        {
            i = 23;
            Console.WriteLine(i);
        }
        public void DoStuff3([Optional] int i)
        {
            Console.WriteLine(i);
        }

        public static void Main()
        {
            new MyClass().DoStuff(); // Error [CS0246]
        }
    }
}
