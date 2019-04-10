using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalSimplification
    {
        object Identity(object o)
        {
            return o;
        }
        object IdentityAnyOtherMethod(object o  )
        {
            return o;
        }
        int Test(object a, object b, object y, bool condition)
        {
            object x;

            if (a != null) // Noncompliant {{Use the '??' operator here.}}
//          ^^
            {
                /*some comment*/
                x = a;
            }
            else
            {
                x = b/*some other comment*/;
            }

            x = a != null ? (a) : b;  // Noncompliant {{Use the '??' operator here.}}
//              ^^^^^^^^^^^^^^^^^^^
            x = a != null ? a : a;  // Compliant, triggers S2758

            int i = 5;
            var z = i == null ? 4 : i; //can't be converted

            x = (y == null) ? Identity(new object()) : Identity(y);  // Noncompliant {{Use the '??' operator here.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            x = a ?? b;
            x = a ?? b;
            x = y ?? new object();
            x = condition ? a : b;

            if (condition) // Noncompliant {{Use the '?:' operator here.}}
            {
                x = a;
            }
            else
            {
                x = b;
            }

            if (condition) // Noncompliant
            {
                x = Identity(new object());
            }
            else
            {
                x = IdentityAnyOtherMethod(y);
            }

            if (condition) // Noncompliant
            {
                Identity(new object());
            }
            else
            {
                Identity(y);
            }

            if (condition) // Noncompliant
            {
                return 1;
            }
            else
            {
                return 2;
            }

            if (condition)
                return 1;
            else if (condition) //Compliant
                return 2;
            else
                return 3;

            X o = null;
            if (o == null) //Non-compliant, but not handled because of the type difference, and there is no fix for it
            {
                x = new Y();
            }
            else
            {
                x = o;
            }

            var yyy = new Y();
            if (condition) //Noncompliant
            {
                x = Identity(new Y());
            }
            else
            {
                x = Identity(yyy);
            }

            if (condition) //Noncompliant
            {
                x = Identity(new Y());
            }
            else
            {
                x = Identity(new X());
            }

            Base elem;
            if (condition) // Non-compliant, but not handled because of the type difference
            {
                elem = new A();
            }
            else
            {
                elem = new B();
            }

            if (condition) //Noncompliant
                x = Identity(new Y());
            else
                x = Identity(yyy);

            if (condition) // Noncompliant
            {
                elem = new A();
            }
            else
            {
                elem = null;
            }
            if (condition) // Non-compliant, but not handled because of the type difference
            {
                elem = new A();
            }
            else
            {
                elem = new NonExistentType(); // Error [CS0246]
            }

            if (false) // Noncompliant
            {
                elem = null;
            }
            else
            {
                elem = (null);
            }
        }

        // we ignore lambdas because of type resolution for conditional expressions, see CS0173
        Action LambdasAreIgnored(bool condition, object a, Action action)
        {
            Action myAction;
            if (false)
            {
                myAction = () => { };
            }
            else
            {
                myAction = () => { Console.WriteLine(); };
            }

            if (condition)
            {
                return () => X();
            }
            else
            {
                return () => Y();
            }

            if (condition)
            {
                Task.Run(() => X());
            }
            else
            {
                Task.Run(() => Y());
            }

            if (condition)
            {
                Bar(s => true);
            }
            else
            {
                Bar(s => false);
            }

            // if one arg is lambda, ignore
            if (condition)
            {
                Foo(1, "2", () => X());
            }
            else
            {
                Foo(1, "2", () => Y());
            }

            Action x;
            if (action != null)
            {
                x = action;
            }
            else
            {
                x = () => Y();
            }
            return null;
        }

        void X() { }
        void Y() { }
        void Foo(int a, string b, Action c) { }
        void Bar(Func<int, bool> func) { }
    }

    class X { }
    class Y { }

    class Base { }
    class A : Base { }
    class B : Base { }

    class T
    {
        public static void XXX()
        {
            string name = "foobar";

            if (name == "")
            {
                Bar(name, null);
            }
            else
            {
                Bar(name, true);
            }

            if (name == "") // Noncompliant
            {
                Bar(((name)), ((false)));
            }
            else
            {
                Bar(name, true);
            }
        }

        private static void Bar(string name, bool? value) { }
    }
}
