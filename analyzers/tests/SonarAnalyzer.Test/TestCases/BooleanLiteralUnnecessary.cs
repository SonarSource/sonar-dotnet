using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class BooleanLiteralUnnecessary
    {
        public BooleanLiteralUnnecessary(bool a, bool b, bool? c, Item item)
        {
            var z = true || ((true));   // Noncompliant {{Remove the unnecessary Boolean literal(s).}}
//                       ^^^^^^^^^^^
            z = (true) || true;     // Noncompliant
//                     ^^^^^^^
            z = false || false;     // Noncompliant (also S2589 and S1764)
            z = true || false;      // Noncompliant
            z = false || true;      // Noncompliant
            z = false && false;     // Noncompliant
            z = false && true;      // Noncompliant
            z = true && true;       // Noncompliant
            z = true && false;      // Noncompliant
            z = true == true;       // Noncompliant
            z = false == true;      // Noncompliant
            z = false == false;     // Noncompliant
            z = true == false;      // Noncompliant
            z = (true == false);    // Noncompliant
            z = true != true;       // Noncompliant (also S1764)
            z = false != true;      // Noncompliant
            z = false != false;     // Noncompliant
            z = true != false;      // Noncompliant
            z = true is true;       // Noncompliant
            z = false is true;      // Noncompliant
            z = false is false;     // Noncompliant
            z = true is false;      // Noncompliant

            var x = !true;                  // Noncompliant
//                   ^^^^
            x = true || false;              // Noncompliant
            x = !false;                     // Noncompliant
            x = (a == false)                // Noncompliant
                && true;                    // Noncompliant
            x = (a is false)                // Noncompliant
                && true;                    // Noncompliant

            x = a is (true);                // Noncompliant
            x = a == true;                  // Noncompliant
            x = a is true;                  // Noncompliant
            x = a != false;                 // Noncompliant
            x = a != true;                  // Noncompliant
            x = false == a;                 // Noncompliant
            x = true == a;                  // Noncompliant
            x = false is a;                 // Error [CS9135]
            x = true is a;                  // Error [CS9135]
            x = false != a;                 // Noncompliant
            x = true != a;                  // Noncompliant
            x = false && Foo();             // Noncompliant
//                    ^^^^^^^^
            x = Foo() && false;             // Noncompliant
//              ^^^^^^^^
            x = true && Foo();             // Noncompliant
//              ^^^^^^^
            x = Foo() && true;             // Noncompliant
//                    ^^^^^^^
            x = Foo() || false;              // Noncompliant
//                    ^^^^^^^^
            x = false || Foo();              // Noncompliant
//              ^^^^^^^^
            x = Foo() || true;              // Noncompliant
//              ^^^^^^^^
            x = true || Foo();              // Noncompliant
//                   ^^^^^^^^
            x = a == true == b;             // Noncompliant

            x = a == Foo(((true)));             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant

            var condition = false;
            var exp = true;
            var exp2 = true;

            var booleanVariable = condition ? ((true)) : exp; // Noncompliant
//                                            ^^^^^^^^
            booleanVariable = condition ? false : exp; // Noncompliant
            booleanVariable = condition ? exp : true; // Noncompliant
            booleanVariable = condition ? exp : false; // Noncompliant
            booleanVariable = condition ? true : false; // Noncompliant
            booleanVariable = !condition ? true : false; // Noncompliant
            booleanVariable = condition ? true : true; // Compliant, this triggers another issue S2758
            booleanVariable = condition ? throw new Exception() : true; // Compliant, we don't raise for throw expressions
            booleanVariable = condition ? throw new Exception() : false; // Compliant, we don't raise for throw expressions

            booleanVariable = condition ? exp : exp2;

            b = x || booleanVariable ? false : true; // Noncompliant

            SomeFunc(true || true); // Noncompliant

            if (c == true) //Compliant
            { }
            if (b is true) // Noncompliant
//                ^^^^^^^
            { }
            if (b is false) // Noncompliant
//                ^^^^^^^^
            { }
            if (c is true) // Compliant
            { }

            var d = true ? c : false;

            var newItem = new Item
            {
                Required = item == null ? false : item.Required // Noncompliant
//                                        ^^^^^
            };
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2618
        public void Repro_2618(Item item)
        {
            var booleanVariable = item is Item myItem ? myItem.Required : false; // Noncompliant
            booleanVariable = item is Item myItem2 ? myItem2.Required : true; // Noncompliant
        }

        public static void SomeFunc(bool x) { }

        private bool Foo()
        {
            return false;
        }

        private bool Foo(bool a)
        {
            return a;
        }

        private bool Bar()
        {
            return false;
        }

        private void M()
        {
            for (int i = 0; true; i++) // Noncompliant
            {
            }
            for (int i = 0; false; i++)
            {
            }
            for (int i = 0; ; i++)
            {
            }

            var b = true;
            for (int i = 0; b; i++)
            {
            }
        }

        private void IsPattern(bool a, bool c)
        {
            const bool b = true;
            a = a is b;
            a = (a is b) ? a : b;
            a = (a is b && c) ? a : b;
            a = a is b && c;

            if (a is bool d
                && a is var e)
            { }
        }

        // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/4465
        private class Repro4465
        {
            public int LiteralInTernaryCondition(bool condition, int result)
            {
                return condition == false
                    ? result
                    : throw new Exception();
            }

            public bool LiteralInTernaryBranch(bool condition)
            {
                return condition
                    ? throw new Exception()
                    : true;
            }

            public void ThrowExpressionIsIgnored(bool condition, int number)
            {
                var x = !condition ? throw new Exception() : false;
                x = number > 0 ? throw new Exception() : false;
                x = condition && condition ? throw new Exception() : false;
                x = condition || condition ? throw new Exception() : false;
                x = condition != true ? throw new Exception() : false;
                x = condition is true ? throw new Exception() : false;
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/7462
        public void Repro_7462(object obj)
        {
            var a = obj == null ? false : obj.ToString() == "x" || obj.ToString() == "y"; // Noncompliant
        }
    }

    public class Item
    {
        public bool Required { get; set; }
    }

    public class SocketContainer
    {
        private IEnumerable<Socket> sockets;
        public bool IsValid =>
            sockets.All(x => x.IsStateValid == true); // Compliant, this failed when we compile with Roslyn 1.x
    }

    public class Socket
    {
        public bool? IsStateValid { get; set; }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7999
    class Repro7999CodeFixError
    {
        void Method(bool cond)
        {
            if (cond == false) { }  // Noncompliant
            if (cond != false) { }  // Noncompliant

            if (cond == true) { }   // Noncompliant
            if (cond != true) { }   // Noncompliant

            if (false == cond) { }  // Noncompliant
            if (false != cond) { }  // Noncompliant

            if (true == cond) { }   // Noncompliant
            if (true != cond) { }   // Noncompliant
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7792
    class ObjectIsBool
    {
        void Object(object obj, Exception exc)
        {
            if (obj is true) { }
            if (exc.Data["SomeKey"] is true) { }
        }

        void ConvertibleToBool(IComparable comparable, IComparable<bool> comparableBool, IEquatable<bool> equatable, IConvertible convertible)
        {
            if (comparable is true) { }
            if (comparableBool is true) { }
            if (equatable is true) { }
            if (convertible is true) { }
        }
    }

    class MaybeBooleans
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/8995
        public void  Dynamic(dynamic bag) // The behavior of a dynamic object can be anything with respect to all kinds of accesses. See ViewBag below for a non-throwing implementation of an DynamicObject
        {
            if (bag.Flag == true) // Noncompliant FP: At runtime bag.Flag will return null if a "ViewBag" is passed.
                                  // if (null == true) can be evaluated at runtime, but
                                  // if (null) not
            { }
        }
        public void NullableBool(bool? flag)
        {
            if (flag == true) // Compliant
            { }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/8995
        public void NullableStruct(YesNo? yesNo)
        {
            if (yesNo == true) // Noncompliant FP: if (yesNo) does not compile CS0266: Cannot implicitly convert type 'YesNo?' to 'bool'.
            { }
        }

        public struct YesNo
        {
            public static implicit operator bool(YesNo yesNo) => true;
        }

        // Mimics the (non-throwing) behavior of
        // https://github.com/dotnet/aspnetcore/blob/1c8f20be1fc4e97044d7ca93edae3af528bc3521/src/Mvc/Mvc.ViewFeatures/src/DynamicViewData.cs#L13
        // See https://github.com/SonarSource/sonar-dotnet/issues/8995
        class ViewBag : DynamicObject
        {
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = null;
                return true;
            }
            public override bool TrySetMember(SetMemberBinder binder, object value) => true;
        }
    }
}
