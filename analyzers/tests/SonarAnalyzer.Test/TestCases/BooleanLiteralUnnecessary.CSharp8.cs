using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5013
    public class CodeFixProviderRepro
    {
        public static bool Foo(string? x, bool y) => x == null ? false : y; // Noncompliant
    }

    // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/4465
    public class Repro4465
    {
        public void Foo(string key)
        {
            var x = (key is null) ? throw new ArgumentNullException(nameof(key)) : false;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7792
    class ConvertibleGenericTypes
    {
        void ConvertibleToBool<T1, T2, T3>(T1 unconstrained, T2 constrainedToStruct, T3 constrainedToBoolInterface)
            where T2 : struct
            where T3 : IComparable<bool>
        {
            if (unconstrained is true) { }
            if (constrainedToStruct is true) { }
            if (constrainedToBoolInterface is true) { }
        }
    }
}
