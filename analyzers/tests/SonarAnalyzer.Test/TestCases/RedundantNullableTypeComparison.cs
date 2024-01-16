using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyClass
    {
        public Type GetType()
        {
            // some other implementation
            return null;
        }
    }

    public class RedundantNullableTypeComparison
    {
        public RedundantNullableTypeComparison()
        {
            int? nullable = 42;
            bool comparison = nullable.GetType() == typeof(Nullable<int>); // Noncompliant, always false
//                            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            comparison = nullable.GetType() != typeof(Nullable<int>); // Noncompliant {{Remove this redundant type comparison.}}

            comparison = new MyClass().GetType() != typeof(Nullable<int>);
            comparison = 42.GetType() != typeof(int);
            comparison = 42.ToString().GetType() != typeof(int);
            comparison = new object() != typeof(Nullable<int>);
        }
    }
}
