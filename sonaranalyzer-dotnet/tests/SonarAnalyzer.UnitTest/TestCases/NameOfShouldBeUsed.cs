using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method_01(int arg1, int arg2)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
//                                  ^^^^^^

            if (arg1 < 0)
                throw new ArgumentException("arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}

            const string foo = "arg1";

            if ("arg1" == foo)
                return;

            throw new ArgumentException("arg1 ");
            throw new ArgumentException("ARG1");
        }

        public Program(int arg1, int arg2)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant
        }

        public bool Method_02(int arg1, int arg1) // Invalid code
        {
            throw new Exception("arg1");
        }
    }
}
