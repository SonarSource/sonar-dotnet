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
        object IdentityAnyOtherMethod(object o)
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

            a = a ?? b;                 // Noncompliant {{Use the '??=' operator here.}}
//          ^^^^^^^^^^
            a = a != null ? (a) : b;    // Noncompliant {{Use the '??=' operator here.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^
            a = null == a ? b : (a);    // Noncompliant {{Use the '??=' operator here.}}
            a = (a ?? b);               // Noncompliant {{Use the '??=' operator here.}}
            a = ((a ?? b));             // Noncompliant {{Use the '??=' operator here.}}
            a = ((null == a ? b : (a)));    // Noncompliant {{Use the '??=' operator here.}}
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^

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

            if ((a != null)) // Noncompliant {{Use the '??' operator here.}}
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

            //This will be CodeFix-ed
            if (a == null) // Noncompliant {{Use the '??=' operator here.}}
            {
                a = b;
            }

            if (a != null)
            {
                a = b;
            }

            bool? value = null;
            if (value == null)  // Noncompliant {{Use the '??=' operator here.}}
                value = false;  //This will be CodeFix-ed and this comment should be preserved

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

            // Removing space from "if (" on next line will fail the test.
            // https://github.com/SonarSource/sonar-dotnet/issues/3064
            if (yyy == null) // Noncompliant
            {
                Identity(new Y());
            }
            else
            {
                Identity(yyy);
            }

            if (((yyy == null))) // Noncompliant
                Identity(new Y());
            else
                Identity(((yyy)));

            if (condition) //Noncompliant
                x = Identity(new Y());
            else
                x = Identity(yyy);

            Base elem;
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

            if (condition) // Non-compliant, but not handled because of the type difference
            {
                elem = new NonExistentType(); // Error [CS0246]
            }
            else
            {
                elem = new A();
            }

            if (false) // Noncompliant
            {
                elem = null;
            }
            else
            {
                elem = (null);
            }

            if (condition)
            {
                Undefined(new object());    // Error [CS0103]: The name 'Undefined' does not exist in the current context
            }
            else
            {
                Undefined(y);               // Error [CS0103]: The name 'Undefined' does not exist in the current context
            }

            if (condition)
            {
                WithIntArg(42);
            }
            else
            {
                WithIntArg(new object());   // Error [CS1503]: Argument 1: cannot convert from 'object' to 'int'
            }

            if (condition)
            {
                Identity(new object());
            }
            else
            {
                x = null;
            }
        }

        object WithIntArg(int arg) => null;

        object IsNull1(object a, object b)
        {
            if (a == null)  // Noncompliant
            {
                return b;
            }
            else
            {
                return a;
            }
        }

        object IsNull2(object a, object b)
        {
            if (a != null)  // Noncompliant
            {
                return a;
            }
            else
            {
                return b;
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

public class Repro_3468
{
    public int NestedTernary(bool condition1, bool condition2, int a, int b, int c)
    {
        if (condition1) // Compliant, changing this will lead to nested conditionals
        {
            return condition2 ? a : b;
        }
        else
        {
            return c;
        }
    }

    public int NestedExpressionWithTernary(bool condition1, bool condition2, int a, int b, int c)
    {
        if (condition1) // Compliant, changing this will lead to nested conditionals
        {
            return 10 + (condition2 ? a : b);
        }
        else
        {
            return c;
        }
    }

    public int NestedSwitch(bool condition1, bool condition2, int a, int b, int c)
    {
        if (condition1) // Compliant, changing this will lead to nested conditionals
        {
            return condition2 switch
            {
                true => a,
                false => b
            };
        }
        else
        {
            return c;
        }
    }

    public int NestedExpressionWithSwitch(bool condition1, bool condition2, int a, int b, int c)
    {
        if (condition1) // Compliant, changing this will lead to nested conditionals
        {
            return 10 + condition2 switch
            {
                true => a,
                false => b
            };
        }
        else
        {
            return c;
        }
    }

    public int? NestedNullCoalescing(bool condition, int? a, int b)
    {
        if (condition) // Noncompliant
        {
            return b;
        }
        else
        {
            return a ?? b;
        }
    }

    public int? NestedNullCoalescingAssignment(bool condition, int? a, int b)
    {
        if (condition) // Noncompliant
        {
            return b;
        }
        else
        {
            return a ??= b;
        }
    }
}
