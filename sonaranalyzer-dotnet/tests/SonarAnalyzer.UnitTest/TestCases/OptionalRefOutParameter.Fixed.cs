using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class OptionalRefOutParameter : Base
    {
        public void DoStuff(ref int i) // Fixed
        {
            Console.WriteLine(i);
        }
        public void DoStuff2(out int i) // Fixed
        {
            Console.WriteLine(i);
        }
        public void DoStuff3([Optional] int i)
        {
            Console.WriteLine(i);
        }

        public static void Main()
        {
            new MyClass().DoStuff(); // This doesn't compile, CS7036 shows
        }
    }
}
