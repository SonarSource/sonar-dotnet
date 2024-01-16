using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method_With_RawStringLiterals(int arg1, string argument)
        {
            if (arg1 < 0)
                throw new Exception("""arg1"""); // Noncompliant
            else if (arg1 < 1)
                throw new ArgumentException("""Bad parameter name""", """arg1"""); // Noncompliant
            else if (arg1 < 2)
                throw new ArgumentOutOfRangeException("""
                    arg1
                    """); // Noncompliant@-2
            else if (arg1 < 3)
                throw new Exception($"""argument {argument}"""); // Noncompliant
//                                      ^^^^^^^^^
            else if (arg1 < 4)
                throw new Exception($"""arg1 {arg1}"""); // Compliant (parameter length 4, minimum allowed to raise 5)
            else
                throw new Exception($"""{nameof(argument)} should not be used with value {argument}"""); // Compliant
        }


        public void Method_With_NewLinesInStringInterpolation(int arg1)
        {
            string argName = "arg1";
            if (arg1 < 0)
            {
                throw new Exception($"{
                    arg1 switch
                    {
                        < 0 => "arg1",  // Noncompliant
                        _ => "Can't touch this",
                    }}");
            }
            else
            {
                throw new Exception($$"""arg1"""); // Noncompliant
            }
        }
    }
}
