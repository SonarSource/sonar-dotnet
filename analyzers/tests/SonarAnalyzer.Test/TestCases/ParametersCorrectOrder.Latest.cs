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

class StaticLocalFunctions
{
    public void M1()
    {
        static double divide(int divisor, int dividend) // Secondary
        {
            return divisor / dividend;
        }

        static void doTheThing(int divisor, int dividend)
        {
            double result = divide(dividend, divisor);  // Noncompliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8071
namespace Repro_8071
{
    class BaseConstructor
    {
        class Base(int a, int b) // Secondary [Base1, Base2, Base3, Base4, Base5, Base6, Base7, Base8]
        {
            Base(int a, int b, string c) : this(b, a) { }                                            // Noncompliant [Base1]
            //                             ^^^^                                                      
            Base(string c, int a, int b) : this(b, a) { }                                            // Noncompliant [Base2]
        }

        class ParamsFullyInverted(int a, int b) : Base(b, a);                                        // Noncompliant [Base3]
        //                                        ^^^^
        class ParamsPartiallyInverted(int a, int b, int c) : BaseConstructor.Base(b, a);             // Noncompliant [Base4]
        //                                                                   ^^^^
        class ParamsPartiallyInvertedWithAdditionalParamAfter(int a, int b, string s) : Base(b, a);  // Noncompliant [Base5]
        class ParamsPartiallyInvertedWithAdditionalParamBefore(string s, int a, int b) : Base(b, a); // Noncompliant [Base6]

        Base MyMethod(int b, int a)
        {
            _ = new Base(b, a);                                                                      // Noncompliant [Base7]
            return new(b, a);                                                                        // Noncompliant [Base8]
        }
    }

    class WithRecordStructs
    {
        void Basics(int a, int b, int c)
        {
            _ = new SomeRecord(b, a);           // Noncompliant [SomeRecord1]
        }

        void WithPromotion(short a, short b)
        {
            SomeRecord _ = new(b, a);           // Noncompliant [SomeRecord2]
            //             ^^^
        }

        void WithCasting(long a, long b)
        {
            _ = new SomeRecord((int)b, (int)a); // FN [SomeRecord3]
        }

        record SomeRecord(int a, int b)  // Secondary [SomeRecord1, SomeRecord2, SomeRecord4, SomeRecord5]
        {
            public SomeRecord(int a, int b, string c) : this(b, a) { } // Noncompliant [SomeRecord4]
            public SomeRecord(string c, int a, int b) : this(b, a) { } // Noncompliant [SomeRecord5]
        }
    }

    class WithRecords
    {
        void Basics(int a, int b, int c)
        {
            _ = new SomeRecordStruct(b, a);           // Noncompliant [SomeRecordStruct1]
        }

        void WithPromotion(short a, short b)
        {
            _ = new SomeRecordStruct(b, a);           // Noncompliant [SomeRecordStruct2]
        }

        void WithCasting(long a, long b)
        {
            _ = new SomeRecordStruct((int)b, (int)a); // FN [SomeRecordStruct3]
        }

        record struct SomeRecordStruct(int a, int b) // Secondary [SomeRecordStruct1, SomeRecordStruct2, SomeRecordStruct4, SomeRecordStruct5]
        {
            public SomeRecordStruct(int a, int b, string c) : this(b, a) { } // Noncompliant [SomeRecordStruct4]
            public SomeRecordStruct(string c, int a, int b) : this(b, a) { } // Noncompliant [SomeRecordStruct5]
        }
    }
}

namespace Repro_8072
{
    public class DefaultLambdaParameters
    {
        void InvokedFromAnotherLambda()
        {
            var f1 = (int a = 42, int b = 42) => a + b;
            var paramsFullyInverted = (int a = 42, int b = 42) => f1(b, a);                                           // FN
            var paramsFullyInvertedWithAdditionalParamAfter = (int a = 42, int b = 42, string s = "42") => f1(b, a);  // FN
            var paramsFullyInvertedWithAdditionalParamBefore = (string s = "42", int a = 42, int b = 42) => f1(b, a); // FN

            var f2 = (int a = 42, int b = 42, int c = 42) => a + b + c;
            var paramsPartiallyInvertedFirstAndSecond = (int a = 42, int b = 42, int c = 42) => f2(b, a, c); // FN
            var paramsPartiallyInvertedFirstAndLast = (int a = 42, int b = 42, int c = 42) => f2(c, b, a);   // FN
            var paramsPartiallyInvertedSecondAndLast = (int a = 42, int b = 42, int c = 42) => f2(a, c, b);  // FN
        }

        void InvokedFromLocalFunction()
        {
            var f = (int a = 42, int b = 42) => a + b;

            int SomeLocalFunction(int a = 42, int b = 42) => f(b, a); // FN
        }
    }
}

public static class NewExtensions
{
    extension(int)
    {
        static double divide(int divisor, int dividend)                 // Secondary
        {
            return divisor / dividend;
        }
    }

    extension(int math)
    {
        public double instanceDivide(int divisor, int dividend)         // Secondary
        {
            return divisor / dividend;
        }
    }

    public class TestExtensions
    {
        void doTheThing(int divisor, int dividend)
        {
            double staticDivide = int.divide(dividend, divisor);        // Noncompliant
            var instanceDivide = 10.instanceDivide(dividend, divisor);  // Noncompliant
        }
    }
}
