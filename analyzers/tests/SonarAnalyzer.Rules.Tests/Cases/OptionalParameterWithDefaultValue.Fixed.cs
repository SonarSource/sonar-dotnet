using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Tests.Diagnostics
{
    public class OptionalParameterWithDefaultValue
    {
        class MyClass
        {
            public void DoStuff([Optional][DefaultParameterValue(4)]int i, int j = 5) // Fixed
            {
                Console.WriteLine(i);
            }

            public void DoStuff1([Optional][DefaultParameterValue(value: 4)]int i, int j = 5) // Fixed
            {
                Console.WriteLine(i);
            }

            public void DoStuff2([Optional][DefaultParameterValue(4)]int i, int j = 5)
            {
                Console.WriteLine(i);
            }

            public void DoStuff3([DefaultValue(4)]int i, int j = 5) // okay, we have no idea what the intent was
            {
                Console.WriteLine(i);
            }

            public void DoStuff4([Optional][DefaultValue(typeof(int), "1")]int i, int j = 5) // Fixed
            {
                Console.WriteLine(i);
            }

            public static void Main()
            {
                new MyClass().DoStuff(); // prints 0
                new MyClass().DoStuff2(); // prints 4
            }

            public void DoStuff5([DefaultParameterValue(4)][DefaultValue(4)]int i, int j = 5) // Compliant, S3450 will trigger
            {
                Console.WriteLine(i);
            }
        }
    }
}
