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

            throw new ArgumentException
            {
                Source = "arg1"                       // Noncompliant
            };

            throw new ArgumentException
            {
                Source = "argument123"
            };

            throw new ArgumentException("argument "); // Noncompliant
            throw new ArgumentException("argument,"); // Noncompliant
            throw new ArgumentException("argument!"); // Noncompliant
            throw new ArgumentException("argument?"); // Noncompliant

            throw new ArgumentException("argument123");
            throw new ArgumentException("ARGUMENT");
            throw new ArgumentException("arg "); // too short name
            throw new ArgumentException("arg!"); // too short name

            throw new ArgumentException("This is argument."); // Noncompliant
            throw new ArgumentException("This is argument.", nameof(argument));
            throw new ArgumentException("argument and arg1"); // Noncompliant
            throw new ArgumentException("argument and arg1", nameof(arg1));

            throw new ArgumentNullException("arg1"); // Noncompliant
            throw new ArgumentNullException(nameof(arg1));
            throw new ArgumentNullException(nameof(argument), "argument");
            throw new ArgumentNullException(nameof(argument), "Incorrect argument value");

            throw new ArgumentOutOfRangeException("arg1"); // Noncompliant
            throw new ArgumentOutOfRangeException(nameof(arg1));
            throw new ArgumentOutOfRangeException(nameof(argument), "argument");
            throw new ArgumentOutOfRangeException(nameof(argument), "Incorrect argument value");
            throw new ArgumentOutOfRangeException(nameof(argument), argument, "argument");
            throw new ArgumentOutOfRangeException(nameof(argument), argument, "Incorrect argument value");

            var ex1 = new ArgumentException("This is argument."); // FN
            throw ex1;
            var ex2 = new ArgumentException("This is argument.", nameof(argument));
            throw ex2;

            throw new ArgumentException(nameof(argument), nameof(arg1), new ArgumentException("argument", nameof(arg1)));
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

        public bool MethodWithUnnecessaryParams(int a, int two, int of, int that)
        {
            throw new Exception("of type");
            throw new Exception("I have a car");
            throw new Exception($"I have two cars");
            throw new Exception($"I have that problem");
        }

        public bool SameStringTokens(int argumentName)
        {
            throw new ArgumentException("argumentName", "argumentName"); // Noncompliant
            // Noncompliant@-1
            throw new Exception("argumentName"); // Noncompliant
            throw new ArgumentException("argumentName argumentName argumentName"); // Noncompliant (only one message)
            throw new ArgumentException("argumentName argumentName argumentName"); // Noncompliant
            throw new Exception($"argumentName argumentName argumentName"); // Noncompliant (only one message)
            throw new Exception($"argumentName argumentName argumentName"); // Noncompliant
            throw new Exception($"argumentName"); // Noncompliant
            throw new Exception($"argumentName"); // Noncompliant
        }
    }
}
