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

            x = a ?? b/*some other comment*/;

            x = a ?? b;  // Fixed
            x = a != null ? a : a;  // Compliant, triggers S2758

            int i = 5;
            var z = i == null ? 4 : i; //can't be converted

            x = Identity(y ?? new object());  // Fixed

            x = a ?? b;
            x = a ?? b;
            x = y ?? new object();
            x = condition ? a : b;

            x = condition ? a : b;

            x = condition ? Identity(new object()) : IdentityAnyOtherMethod(y);

            Identity(condition ? new object() : y);

            return condition ? 1 : 2;

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
            x = condition ? Identity(new Y()) : Identity(yyy);

            x = condition ? Identity(new Y()) : Identity(new X());

            Base elem;
            if (condition) // Non-compliant, but not handled because of the type difference
            {
                elem = new A();
            }
            else
            {
                elem = new B();
            }

            x = condition ? Identity(new Y()) : Identity(yyy);

            elem = condition ? new A() : null;
            if (condition) // Non-compliant, but not handled because of the type difference
            {
                elem = new A();
            }
            else
            {
                elem = new NonExistentType(); // Error [CS0246]
            }

            elem = false ? null : (null);
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

            Bar(name, name == "" ? false : true);
        }

        private static void Bar(string name, bool? value) { }
    }
}
