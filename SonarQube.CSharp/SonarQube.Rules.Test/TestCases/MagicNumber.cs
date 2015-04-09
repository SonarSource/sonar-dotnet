using System.Runtime.InteropServices;

#pragma warning disable 659 // overrides AddToHashCodeCombiner instead

namespace Tests.Diagnostics
{
    public class MagicNumber
    {
        public int foo = 42;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string v2 = "";

        public MagicNumber()
        {
            const decimal grossSalary = 80000;
            const decimal Rate = 0.85m;

            decimal rate2 = 0.85m;

            decimal netSalary1;
            decimal netSalary2;

            netSalary1 = Rate * grossSalary;
            netSalary2 = 0.85m * grossSalary; // Noncompliant
            netSalary2 = 42 * grossSalary; // Noncompliant
            netSalary2 = 42L * grossSalary; // Noncompliant

            netSalary1 = 0;
            netSalary1 = -1;
            netSalary1 = +1;
            netSalary1 = 0x0;
            netSalary1 = 0x00;
            netSalary1 = (decimal).1;
        }

        public enum Foo
        {
            a = 0,
            b = 1,
            c = 2,
            d = 3,
            e = 4
        }
    }
}
