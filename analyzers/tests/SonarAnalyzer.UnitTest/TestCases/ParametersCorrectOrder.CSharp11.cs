using System;

namespace Tests.Diagnostics
{
    public interface IInterface
    {
        static virtual void SomeMethod(int a, int b) { } // Secondary
    }

    public class SomeClass<T> where T : IInterface
    {
        public SomeClass()
        {
            int a = 1;
            int b = 2;

            T.SomeMethod(b, a); // Noncompliant
        }
    }
}

namespace Repro_8072
{
    class Lambdas
    {
        Func<int, int, int> F1 = (int a, int b) => a + b;

        void InvokedFromAnotherLambda()
        {
            var f1 = (int a, int b) => a + b;
            var paramsFullyInverted = (int a, int b) => f1(b, a);                                    // FN
            var paramsFullyInvertedWithAdditionalParamAfter = (int a, int b, string s) => f1(b, a);  // FN
            var paramsFullyInvertedWithAdditionalParamBefore = (string s, int a, int b) => f1(b, a); // FN

            var f2 = (int a, int b, int c) => a + b + c;
            var paramsPartiallyInvertedFirstAndSecond = (int a, int b, int c) => f2(b, a, c);        // FN
            var paramsPartiallyInvertedFirstAndLast = (int a, int b, int c) => f2(c, b, a);          // FN
            var paramsPartiallyInvertedSecondAndLast = (int a, int b, int c) => f2(a, c, b);         // FN
        }

        void InvokedFromLocalFunction()
        {
            var f1 = (int a, int b) => a + b;
            var f2 = (int a, int b, int c) => a + b + c;

            int FullyInverted(int a, int b) => f1(b, a);                                    // FN
            int FullyInvertedWithAdditionalParamAfter(int a, int b, string c) => f1(b, a);  // FN
            int FullyInvertedWithAdditionalParamBefore(string c, int a, int b) => f1(b, a); // FN

            int PartiallyInvertedFirstAndSecond(int a, int b, int c) => f2(b, a, c); // FN
            int PartiallyInvertedFirstAndLast(int a, int b, int c) => f2(c, b, a);   // FN
            int PartiallyInvertedSecondAndLast(int a, int b, int c) => f2(a, c, b);  // FN
        }

        void InvokedFromAMethod(int a, int b)
        {
            F1(b, a);   // FN
        }
    }
}
