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

            ValidateArgument(arg1, "arg1");

            if ("arg1" == foo)
                return;

            throw new ArgumentException("arg1 ");
            throw new ArgumentException("ARG1");
        }

        private void ValidateArgument(int v, string name)
        {
            if (v < 0)
                throw new ArgumentOutOfRangeException("name"); // Noncompliant
            if (v < 0)
                throw new Exception(nameof(name));
        }

        public Program(int arg1, int arg2)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant
        }

        public bool Method_02(int arg1, int arg1) // Error [CS0100] - duplicated args
        {
            throw new Exception("arg1");
        }

    }
}
