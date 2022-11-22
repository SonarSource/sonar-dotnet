using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method_With_RawStringLiterals(int arg1)
        {
            if (arg1 < 0)
                throw new Exception("""arg1"""); // FN
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
            if (arg1 == 0)
            {
                string arg1Name = "arg1";
                throw new Exception($"{arg1}"); // FN
            }
        }
    }
}
