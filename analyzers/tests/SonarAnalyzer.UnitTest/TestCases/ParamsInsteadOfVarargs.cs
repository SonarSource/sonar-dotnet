namespace Tests.Diagnostics
{
    class Program
    {
        public static void TestFunc(__arglist) // Noncompliant {{VarArgs calling convention is not CLS compliant, use the 'params' keyword instead}}
        {
        }

        public static void TestFunc(int firstVar, string secondVar, __arglist) // Noncompliant
        {
        }

    }

}
