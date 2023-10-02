using System;

// https://github.com/SonarSource/sonar-dotnet/issues/8071
namespace Repro_8071
{
    class BaseConstructor
    {
        class Base(int a, int b)
        {
            Base(int a, int b, string c) : this(b, a) { }  // FN: ctor params inverted with additional param after
            Base(string c, int a, int b) : this(b, a) { }  // FN: ctor params inverted with additional param before
        }

        class ParamsFullyInverted(int a, int b) : Base(b, a);                                    // FN
        class ParamsPartiallyInverted(int a, int b, int c) : Base(b, a);                         // FN
        class ParamsFullyInvertedWithAdditionalParamAfter(int a, int b, string s) : Base(b, a);  // FN
        class ParamsFullyInvertedWithAdditionalParamBefore(string s, int a, int b) : Base(b, a); // FN
    }

    class WithRecordStructs
    {
        void Basics(int a, int b, int c)
        {
            _ = new SomeRecord(b, a);           // Noncompliant
        }

        void WithPromotion(short a, short b)
        {
            _ = new SomeRecord(b, a);           // Noncompliant
        }

        void WithCasting(long a, long b)
        {
            _ = new SomeRecord((int)b, (int)a); // FN
        }

        record SomeRecord(int a, int b)
        {
            public SomeRecord(int a, int b, string c) : this(b, a) { } // FN
            public SomeRecord(string c, int a, int b) : this(b, a) { } // FN
        }
    }

    class WithRecords
    {
        void Basics(int a, int b, int c)
        {
            _ = new SomeRecordStruct(b, a);           // Noncompliant
        }

        void WithPromotion(short a, short b)
        {
            _ = new SomeRecordStruct(b, a);           // Noncompliant
        }

        void WithCasting(long a, long b)
        {
            _ = new SomeRecordStruct((int)b, (int)a); // FN
        }

        record struct SomeRecordStruct(int a, int b)
        {
            public SomeRecordStruct(int a, int b, string c) : this(b, a) { } // FN
            public SomeRecordStruct(string c, int a, int b) : this(b, a) { } // FN
        }
    }
}

namespace Repro_8072
{
    public class DefaultLambdaParameters
    {
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
            var f = (int a, int b) => a + b;

            int SomeLocalFunction(int a, int b) => f(b, a);
        }
    }
}
