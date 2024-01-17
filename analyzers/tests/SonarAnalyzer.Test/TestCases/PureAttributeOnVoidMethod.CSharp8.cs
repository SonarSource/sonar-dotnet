using System;
using System.Diagnostics.Contracts;

namespace Tests.TestCases
{
    public class PureAttributes
    {
        private int age;

        [Pure] // Noncompliant
        void WithExplicitInParamater(in int age)
        {
            this.age = age;
        }
    }
}
