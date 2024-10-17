using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5013
    public class CodeFixProviderRepro
    {
        public static bool Foo(string? x, bool y) => !(x == null) && y; // Fixed
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
            if (unconstrained is true)
            { }
            if (constrainedToStruct is true)
            { }
            if (constrainedToBoolInterface is true)
            { }
        }
    }

    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5219
    public class Repro
    {
        void Reproducers(bool condition)
        {
            bool? v1 = condition ? true : null;

            bool? v2 = condition ? null : true;

            bool? v3 = condition ? true : SomeMethod();

            bool? v4 = condition || SomeMethod2(); // Fixed

            bool? v5 = condition || SomeMethod2();
        }

        public bool? SomeMethod()
        {
            return null;
        }

        public bool SomeMethod2()
        {
            return true;
        }

        // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7688
        void IsNotPattern(bool a, bool? b)
        {
            _ = true;          // Fixed
            _ = true;          // Fixed
            _ = false;           // Fixed
            _ = false;         // Fixed
            _ = true;     // Fixed
            _ = false; // Fixed

            _ = !a;     // Fixed
            _ = !a;     // Fixed
            _ = a; // Fixed

            if (!a) // Fixed
            { }
            if (a) // Fixed
            { }
            if (b is not true) // Compliant
            { }
            if (a is { } myVar) // Compliant
            { }

            const bool c = true;
            a = a is not c;
            a = (a is not c) ? a : c;
            a = (a is not c && a) ? a : c;
            a = a is not c && a;

            var x = a is not true ? throw new Exception() : false;
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2618
        public void Repro_2618(Item item)
        {
            var booleanVariable = !(item is not Item myItem) && myItem.Required; // Fixed
            booleanVariable = item is not Item myItem2 || myItem2.Required; // Fixed
        }
    }

    public class Item
    {
        public bool Required { get; set; }
    }

    public class NullableWarningSuppression
    {
        public void Test(bool b)
        {
            _ = true;   // Fixed
            _ = true;   // Fixed

            _ = true!;   // Fixed
            _ = false!;  // Fixed
            _ = false!; // Fixed
            _ = false!; // Fixed

            _ = (false)!;   // Fixed
        }
    }
}
