using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Fruit { }

    class Program
    {
        private object someField;

        public void Foo(Object x)
        {
            if (x is Fruit)  // Noncompliant
            {
                var f1 = (Fruit)x;
//                       ^^^^^^^^ Secondary
                var f2 = (Fruit)x;
//                       ^^^^^^^^ Secondary
            }

            var f = x as Fruit;
            if (x != null) // Compliant
            {

            }
        }

        public void Bar(object x)
        {
            if (!(x is Fruit))
            {
                var f1 = (Fruit)x; // Compliant - but will throw
            }
            else
            {
                var f2 = (Fruit)x; // Compliant - should be non compliant
            }

        }

        public void FooBar(object x)
        {
            if (x is int)
            {
                var res = (int)x; // Compliant because we are casting to a ValueType
            }
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2314
        public void TakeIdentifierIntoAccount(object x)
        {
            if (x is Fruit)
            {
                var f = new Fruit();
                var c = (Fruit)f;
            }

            if (someField is Fruit) // Noncompliant
            {
                var fruit = (Fruit)this.someField;
//                          ^^^^^^^^^^^^^^^^^^^^^ Secondary
            }
        }

        public void UnknownFoo(object x)
        {
            if (x is Car)  // Compliant because we ignore what the type is // Error [CS0246]
            {
                var c = (Car)x; // Error [CS0246]
            }
        }
    }
}
