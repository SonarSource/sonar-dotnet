using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Foo
    {
        private string field;

        string Method(string s) =>
            s != null
            ? null
            : s.ToLower(); // Noncompliant

        string Prop =>
            field != null
            ? null
            : field.ToLower(); // Noncompliant

        string PropGet
        {
            get =>
                field != null
                ? null
                : field.ToLower(); // Noncompliant
        }
    }
}
