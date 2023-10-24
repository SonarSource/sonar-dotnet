using System;

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
