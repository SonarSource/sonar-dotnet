﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    // Repro for: https://github.com/SonarSource/sonar-dotnet/issues/5219
    public class Repro
    {
        void Reproducers(bool condition)
        {
            bool? v1 = condition ? true : null;

            bool? v2 = condition ? null : true;

            bool? v3 = condition ? true : SomeMethod();

            bool? v4 = condition ? true : SomeMethod2(); // Noncompliant

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
            _ = true is not false;          // Noncompliant {{Remove the unnecessary Boolean literal(s).}}
            _ = false is not true;          // Noncompliant
            _ = true is not true;           // Noncompliant
            _ = false is not false;         // Noncompliant
            _ = false is not not false;     // Noncompliant
            _ = false is not not not false; // Noncompliant

            _ = a is (not true);     // Noncompliant
            _ = a is not (true);     // Noncompliant
            _ = a is not (not true); // Noncompliant

            if (a is not true) // Noncompliant
//                ^^^^^^^^^^^
            { }
            if (a is not false) // Noncompliant
//                ^^^^^^^^^^^^
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
            var booleanVariable = item is not Item myItem ? false : myItem.Required; // Noncompliant
            booleanVariable = item is not Item myItem2 ? true : myItem2.Required; // Noncompliant
        }
    }

    public class Item
    {
        public bool Required { get; set; }
    }
}
