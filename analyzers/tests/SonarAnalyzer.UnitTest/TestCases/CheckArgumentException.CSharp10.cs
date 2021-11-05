using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string Part1 = "One";
        public const string Part2 = "Two";
        public const string ParamName = $"{Part1}{Part2}";
        public const string WrongParamName = $"{Part2}{Part1}";

        void Foo1(int OneTwo)
        {
            throw new ArgumentNullException(ParamName); // Compliant
        }

        void Foo2(int OneTwo)
        {
            throw new ArgumentNullException(WrongParamName); // Noncompliant
        }
    }
}
