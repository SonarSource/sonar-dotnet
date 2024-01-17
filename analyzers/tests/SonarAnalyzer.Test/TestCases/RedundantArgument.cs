using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Tests.Diagnostics
{
    public static class RedundantArgument
    {
        public static void M(int x, int y = 5, int z = 7) { /* ... */ }
        public static void M2(int x, int y, int z) { /* ... */ }
        public static void M3(int x = 1, int y = 5, int z = 7) { /* ... */ }
        public static void Ext(this int self, int y = 5, params int[] parameters) { }
        public static void Ext2(this int self, int y, params int[] parameters) { }

        public static void Test()
        {
            var x = "".ToString();
            M(1, 5); //Noncompliant, y has the default value
            //   ^
            M(1, 5, 0); //Noncompliant, y has the default value
            //   ^
            M(1, z: 7); //Noncompliant, z has the default value
            M(1, 5, // Noncompliant
//               ^
                7); // Noncompliant, y, z has the default value
//              ^
            M(1);
            M(1, 2, 4);
            M2(1, 1, 1);
            5.Ext(5);       //Noncompliant
            5.Ext(5, 0, 0); //Noncompliant
            5.Ext2(5);

            RedundantArgument.Ext(5, 5, 4, 4, 5, 6); //Noncompliant {{Remove this default value assigned to parameter 'y'.}}
            RedundantArgument.Ext(5, y: 5, parameters: new int[] { 4, 4, 5, 6 }); //Noncompliant {{Remove this default value assigned to parameter 'y'.}}
            RedundantArgument.Ext(5, 5); //Noncompliant

            M3(1,      //Noncompliant
                y: 5,  //Noncompliant
                z: 7); //Noncompliant

            M3(1,      //Noncompliant
                y: 4);
            M3(x: 1,   //Noncompliant
                y: 4);
            M3(1,      //Noncompliant
                4,
                7);    //Noncompliant
            M3(1,      //Noncompliant
                4);
        }
    }

    // Issue #789: Cannot use optional arguments when using expression trees (CS0584)
    public class RedundantArgsInExpressionTrees
    {
        private static string FuncWithOptionals(string str = null, params string[] args)
        {
            return str;
        }

        // Field declaration -> variable declaration
        Func<string> normalField = () => FuncWithOptionals(null, "111", "222"); //Noncompliant -- non-expression tree, so can use defaults
        readonly Expression<Action> expTreeField = () => FuncWithOptionals(null); //Compliant - expression tree, so cannot use defaults

        // Property declaration
        Func<string> normalProperty => () => FuncWithOptionals(str: null); //Noncompliant
        Expression<Action> expTreeProperty => () => FuncWithOptionals(null); //Compliant

        void takeExpression(Expression<Func<string>> expr) { }

        public void Method1()
        {
            // Variable declaration
            Func<string> var1 = () => FuncWithOptionals(null); //Noncompliant
            Expression<Action> expTreeVar = () => FuncWithOptionals(null); //Compliant
            takeExpression(() => FuncWithOptionals(null)); //Compliant

            // Simple assigment
            var1 = () => FuncWithOptionals(str: null, args: "123"); //Noncompliant
            expTreeVar = () => FuncWithOptionals(null); //Compliant
        }
    }

    public interface IInterfaceWithDefaultMethod
    {
        public void Write(int i, int j = 5)
        {
        }
    }

    public class Consumer
    {
        public Consumer(IInterfaceWithDefaultMethod i)
        {
            i.Write(1, 5); // Noncompliant {{Remove this default value assigned to parameter 'j'.}}
        }
    }

    public class WithLocalFunctions
    {
        public void Method()
        {
            Foo(1, 5); // Noncompliant {{Remove this default value assigned to parameter 'j'.}}
            Bar(1, 5); // Noncompliant {{Remove this default value assigned to parameter 'j'.}}

            void Foo(int i, int j = 5)
            {
            }

            static void Bar(int i, int j = 5)
            {
            }
        }
    }
}
