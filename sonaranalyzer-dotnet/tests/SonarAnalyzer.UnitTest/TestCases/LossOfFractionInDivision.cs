using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class LossOfFractionInDivision
    {
        static void Main()
        {
            decimal dec = 3 / 2; // Noncompliant
//                        ^^^^^
            dec = 3L / 2; // Noncompliant
            Method(3 / 2); // Noncompliant
            dec = (decimal)3 / 2;
            Method(3.0F / 2);

            Action<float> a = (f) => { };
            a(3 / 2); // Noncompliant

            ((Action<float>)(f => { }))(3 / 2); // Noncompliant {{Cast one of the operands of this division to 'float'.}}
        }

        static void Method(float f) { }

        static double Calc()
        {
            Func<float> f = () => { return 3 / 2; }; // Noncompliant
            f = () => 3 / 2; // Noncompliant
            var x = 3 / 2;
            return 3 / 2; // Noncompliant
        }

        static double GetCalc
        {
            get
            {
                return 3 / 2; // Noncompliant
            }
        }
    }
}
