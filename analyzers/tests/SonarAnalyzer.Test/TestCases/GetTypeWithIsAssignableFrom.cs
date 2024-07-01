using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class GetTypeWithIsAssignableFrom
    {
        void Test(bool b)
        {
            var expr1 = new GetTypeWithIsAssignableFrom();
            var expr2 = new GetTypeWithIsAssignableFrom();

            if (expr1.GetType()/*abcd*/.IsAssignableFrom(expr2.GetType() /*efgh*/)) // Noncompliant {{Use the 'IsInstanceOfType()' method instead.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }
            if (expr1.GetType().IsInstanceOfType(expr2)) // Compliant
            { }

            if (!typeof(GetTypeWithIsAssignableFrom).IsAssignableFrom(expr1.GetType())) // Noncompliant {{Use the 'is' operator instead.}}
            { }
            var x = typeof(GetTypeWithIsAssignableFrom).IsAssignableFrom(expr1.GetType()); // Noncompliant
            if (expr1 is GetTypeWithIsAssignableFrom) // Noncompliant  {{Use a 'null' check instead.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            { }

            if (typeof(GetTypeWithIsAssignableFrom).IsAssignableFrom(typeof(GetTypeWithIsAssignableFrom))) // Compliant
            { }

            var t1 = expr1.GetType();
            var t2 = expr2.GetType();
            if (t1.IsAssignableFrom(t2)) // Compliant
            { }
            if (t1.IsAssignableFrom(c: expr2.GetType())) // Noncompliant
            { }

            if (t1.IsAssignableFrom(typeof(GetTypeWithIsAssignableFrom))) // Compliant
            { }

            Test(t1.IsAssignableFrom(c: expr2.GetType())); // Noncompliant

            if (expr1 is object) // Compliant - "is object" is a commonly used pattern for non-null check
            { }

            if (expr1 is System.Object) // Compliant - "is object" is a commonly used pattern for non-null check
            { }
        }
    }
    class Fruit { }
    sealed class Apple : Fruit { }
    class NonsealedBerry : Fruit { }

    class Program
    {
        static void Main()
        {
            var apple = new Apple();
            var berry = new NonsealedBerry();
            var b = apple.GetType() == typeof(Apple);       // Noncompliant
            b = berry.GetType() == typeof(NonsealedBerry);  // Compliant, nonsealed class
            b = typeof(Apple).IsInstanceOfType(apple);      // Noncompliant
            b = typeof(Apple).IsAssignableFrom(apple.GetType()); // Noncompliant
            b = typeof(Apple).IsInstanceOfType(apple);      // Noncompliant
            var appleType = typeof(Apple);
            b = appleType.IsAssignableFrom(apple.GetType());    // Noncompliant

            b = apple.GetType() == typeof(int?);    // Compliant

            Fruit f = apple;
            b = true && (((f as Apple)) != null);   // Noncompliant
            b = f as Apple == null;                 // Noncompliant
            b = f as Apple == new Apple();

            b = true && ((apple)) is Apple; // Noncompliant
            b = !(apple is Apple);          // Noncompliant
            b = f is Apple;

            var num = 5;
            b = num is int?;
            b = num is float;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3605
    public class Repro_3605
    {
        public string StringProperty { get; set; }
        public int IntProperty { get; set; }
        public AnEnum EnumProperty { get; set; }

        public const string stringField = "Lorem Ipsum";
        public const int intField = 1;

        public void Go(Repro_3605 value)
        {
            bool result = value.StringProperty is stringField; // Compliant, for pattern matching
            result = value.IntProperty is intField;            // Compliant, for pattern matching
            result = value.EnumProperty is AnEnum.Zero;        // Compliant, for pattern matching
        }
    }

    public enum AnEnum
    {
        Zero = 0
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/6616
    public class Repro_6616
    {
        public void IsInstanceOfType(object obj, Type t)
        {
            _ = typeof(ISet<int>).IsInstanceOfType(obj);                            // Noncompliant, bounded generic type
            _ = typeof(ISet<>).IsInstanceOfType(obj);                               // Compliant, unbonded generic type
            _ = typeof(IDictionary<int, int>).IsInstanceOfType(obj);                // Noncompliant, bounded generic type
            _ = typeof(IDictionary<,>).IsInstanceOfType(obj);                       // Compliant, unbonded generic type
            _ = typeof(System.Collections.Generic.ISet<int>).IsInstanceOfType(obj); // Noncompliant, bounded generic type
            _ = typeof(System.Collections.Generic.ISet<>).IsInstanceOfType(obj);    // Compliant, unbonded generic type

            _ = t.IsInstanceOfType(obj);                                            // Compliant, not a typeof expression
            t = typeof(ISet<>);
            _ = t.IsInstanceOfType(obj);                                            // Compliant, not a typeof expression and value not tracked

            // Error@+1 [CS7003] Unexpected use of an unbound generic name
            _ = typeof(ISet<ISet<>>).IsInstanceOfType(obj);                         // Noncompliant, FP: omitted type nested
            // Error@+1 [CS7003] Unexpected use of an unbound generic name
            _ = typeof(IDictionary<string, ISet<>>).IsInstanceOfType(obj);          // Noncompliant, FP: omitted type nested
        }

        public void IsAssignableFrom(object obj, Type t1, Type t2)
        {
            _ = typeof(HashSet<int>).IsAssignableFrom(obj.GetType());                         // Noncompliant, bounded generic type
            _ = typeof(HashSet<>).IsAssignableFrom(obj.GetType());                            // Compliant, unbonded generic type
            _ = typeof(Dictionary<int, int>).IsAssignableFrom(obj.GetType());                 // Noncompliant, bounded generic type
            _ = typeof(Dictionary<,>).IsAssignableFrom(obj.GetType());                        // Compliant, unbonded generic type
            _ = typeof(System.Collections.Generic.ISet<int>).IsAssignableFrom(obj.GetType()); // Noncompliant, bounded generic type
            _ = typeof(System.Collections.Generic.ISet<>).IsAssignableFrom(obj.GetType());    // Compliant, unbonded generic type

            _ = t1.IsAssignableFrom(t2);                                                      // Compliant, not a typeof expression, nor having GetType as arg
            t1 = typeof(ISet<>);
            t2 = obj.GetType();
            _ = t1.IsAssignableFrom(t2);                                                      // Compliant, not a typeof expression, nor having GetType as arg, and values not tracked

            // Error@+1 [CS7003] Unexpected use of an unbound generic name
            _ = typeof(ISet<ISet<>>).IsAssignableFrom(obj.GetType());                         // Noncompliant, FP: omitted type nested
            // Error@+1 [CS7003] Unexpected use of an unbound generic name
            _ = typeof(IDictionary<string, ISet<>>).IsAssignableFrom(obj.GetType());          // Noncompliant, FP: omitted type nested
        }
    }

    public class Coverage
    {
        public void Foo()
        {
            var b = typeof(Apple).IsEquivalentTo(null);
            this.IsInstanceOfType("x");
            this.IsAssignableFrom("x");
            this.GetType(null);
            var c = this.GetType() == typeof(Apple);
            var d = GetType() == null;
        }

        public bool IsInstanceOfType(string x) => true;
        public bool IsAssignableFrom(string x) => true;
        public bool GetType(object x) => true;
        public Type GetType() => null;
    }

    public class CoverageWithErrors
    {
        public void Go(CoverageWithErrors arg)
        {
            bool b;
            this.IsInstanceOfType("x");                 // Error [CS1061]: 'Coverage2' does not contain a definition for 'IsInstanceOfType'
            b = arg is UndefinedType;                   // Error [CS0246]: The type or namespace name 'UndefinedType' could not be found
            b = undefined is CoverageWithErrors;        // Error [CS0103]: The name 'undefined' does not exist in the current context
            b = arg.GetType() == typeof(UndefinedType); // Error [CS0246]: The type or namespace name 'UndefinedType' could not be found
            b = arg.GetType() == typeof();              // Error [CS1031]: Type expected
            b = arg.GetType() == typeof;                // Error [CS1031]: Type expected
                                                        // Error@-1 [CS1003]: Syntax error, '(' expected
                                                        // Error@-2 [CS1026]: ) expected
        }
    }
}
