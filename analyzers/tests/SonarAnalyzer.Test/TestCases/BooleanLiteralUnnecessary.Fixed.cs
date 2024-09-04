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
            var z = true;   // Fixed
            z = true;     // Fixed
            z = false;     // Fixed
            z = true;      // Fixed
            z = true;      // Fixed
            z = false;     // Fixed
            z = false;      // Fixed
            z = true;       // Fixed
            z = false;      // Fixed
            z = true;       // Fixed
            z = false;      // Fixed
            z = true;     // Fixed
            z = false;      // Fixed
            z = (false);    // Fixed
            z = false;       // Fixed
            z = true;      // Fixed
            z = false;     // Fixed
            z = true;      // Fixed
            z = true;       // Fixed
            z = false;      // Fixed
            z = true;     // Fixed
            z = false;      // Fixed

            var x = false;                  // Fixed
            x = true;              // Fixed
            x = true;                     // Fixed
            x = !a;                    // Fixed
            x = !a;                    // Fixed

            x = a;                // Fixed
            x = a;                  // Fixed
            x = a;                  // Fixed
            x = a;                 // Fixed
            x = !a;                  // Fixed
            x = !a;                 // Fixed
            x = a;                  // Fixed
            x = false is a;                 // Error [CS9135]
            x = true is a;                  // Error [CS9135]
            x = a;                 // Fixed
            x = !a;                  // Fixed
            x = false;             // Fixed
            x = false;             // Fixed
            x = Foo();             // Fixed
            x = Foo();             // Fixed
            x = Foo();              // Fixed
            x = Foo();              // Fixed
            x = true;              // Fixed
            x = true;              // Fixed
            x = a == b;             // Fixed

            x = a == Foo(((true)));             // Compliant
            x = !a;                         // Compliant
            x = Foo() && Bar();             // Compliant

            var condition = false;
            var exp = true;
            var exp2 = true;

            var booleanVariable = condition || exp; // Fixed
            booleanVariable = !condition && exp; // Fixed
            booleanVariable = !condition || exp; // Fixed
            booleanVariable = condition && exp; // Fixed
            booleanVariable = condition; // Fixed
            booleanVariable = !condition; // Fixed
            booleanVariable = condition ? true : true; // Compliant, this triggers another issue S2758
            booleanVariable = condition ? throw new Exception() : true; // Compliant, we don't raise for throw expressions
            booleanVariable = condition ? throw new Exception() : false; // Compliant, we don't raise for throw expressions

            booleanVariable = condition ? exp : exp2;

            b = !(x || booleanVariable); // Fixed

            SomeFunc(true); // Fixed

            if (c == true) //Compliant
            { }
            if (b) // Fixed
            { }
            if (!b) // Fixed
            { }
            if (c is true) // Compliant
            { }

            var d = true ? c : false;

            var newItem = new Item
            {
                Required = !(item == null) && item.Required // Fixed

            };
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/2618
        public void Repro_2618(Item item)
        {
            var booleanVariable = item is Item myItem && myItem.Required; // Fixed
            booleanVariable = !(item is Item myItem2) || myItem2.Required; // Fixed
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
            for (int i = 0; ; i++) // Fixed
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
            var a = !(obj == null) && (obj.ToString() == "x" || obj.ToString() == "y"); // Fixed
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
            if (!cond) { }  // Fixed
            if (cond) { }  // Fixed

            if (cond) { }   // Fixed
            if (!cond) { }   // Fixed

            if (!cond) { }  // Fixed
            if (cond) { }  // Fixed

            if (cond) { }   // Fixed
            if (!cond) { }   // Fixed
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
            if (bag.Flag) // Fixed
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
            if (yesNo) // Fixed
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
