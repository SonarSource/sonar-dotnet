using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public int Method_01(int arg1, int arg2)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
//                                  ^^^^^^
            const string foo = "arg1"; // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}

            if ("arg1" == foo) // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
                return 1;

            var s1 = "arg1 ";
            var s2 = "ARG1";

            // Noncompliant@+1 {{Replace the string 'arg1' with 'nameof(arg1)'.}}
            return @"arg1" == $"arg2" // Noncompliant {{Replace the string 'arg2' with 'nameof(arg2)'.}}
                ? 42 : 42;
        }

        public bool Method_02(int arg1, int arg1) // Invalid code
        {
            return arg1 == "arg1";
        }
    }
}
