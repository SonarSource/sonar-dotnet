using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method_01(int arg1, int argument)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}
//                                  ^^^^^^

            if (arg1 < 0)
                throw new ArgumentException("arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}

            if (arg1 < 0)
                throw new ArgumentException("Bad parameter name", "arg1"); // Noncompliant {{Replace the string 'arg1' with 'nameof(arg1)'.}}

            if (arg1 < 0)
                throw new ArgumentException("argument", "arg1"); // Noncompliant {{Replace the string 'argument' with 'nameof(argument)'.}}
                                                                 // Noncompliant@-1 {{Replace the string 'arg1' with 'nameof(arg1)'.}}

            const string foo = "arg1";

            ValidateArgument(arg1, "arg1");

            if ("arg1" == foo)
                return;

            throw new ArgumentException("argument "); // Noncompliant
            throw new ArgumentException("argument,"); // Noncompliant
            throw new ArgumentException("This is argument."); // Noncompliant  {{Replace the string 'argument' with 'nameof(argument)'.}}
//                                      ^^^^^^^^^^^^^^^^^^^
            throw new ArgumentException("argument!"); // Noncompliant
            throw new ArgumentException("argument?"); // Noncompliant
            throw new ArgumentException("argument and arg2"); // Noncompliant

            throw new ArgumentException("argument123");
            throw new ArgumentException("ARGUMENT");
            throw new ArgumentException("arg "); // too short name
            throw new ArgumentException("arg!"); // too short name
        }

        private void ValidateArgument(int v, string longName)
        {
            if (v < 0)
                throw new ArgumentOutOfRangeException("longName"); // Noncompliant
            if (v < 0)
                throw new Exception(nameof(longName));
            if (v < 0)
                throw new Exception($"{nameof(longName)} should not be used with value {longName}");
        }

        public Program(int arg1, int arg2)
        {
            if (arg1 < 0)
                throw new Exception("arg1"); // Noncompliant
        }

        public Program(string argument, int arg2, object anotherArgument)
        {
            if (argument == null || argument.Length == 0)
                throw new Exception($"Argument argument with value \"{argument}\" is not valid"); // Noncompliant  {{Replace the string 'argument' with 'nameof(argument)'.}}
//                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            throw new Exception($"argument"); // Noncompliant
            throw new Exception($"anotherArgument argument argument value \"{argument}\" is not valid"); // Noncompliant  {{Replace the string 'argument' with 'nameof(argument)'.}}
            throw new Exception("argument anotherArgument argument anotherArgument");  // Noncompliant  {{Replace the string 'argument' with 'nameof(argument)'.}}
        }

        public bool Method_02(int arg1, int arg1) // Error [CS0100] - duplicated args
        {
            throw new Exception("arg1");
        }

        public bool MethodWithSillyParams(int a, int two, int of, int that)
        {
            throw new Exception("of type");
            throw new Exception("I have a car");
            throw new Exception($"I have two cars");
            throw new Exception($"I have that problem");
        }
    }
}
