using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo1()
//           ^^^^ Secondary
//           ^^^^ Secondary@-1
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        void Foo2() // Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
//           ^^^^
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        void Foo3() // Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        void Foo4()
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
            Console.WriteLine("different");
        }



        void Bar1()
        {
            throw new NotImplementedException();
        }

        void Bar2()
        {
            throw new NotImplementedException();
        }



        void FooBar1()
        {
            throw new NotSupportedException();
        }

        void FooBar2()
        {
            throw new NotSupportedException();
        }



        int Baz1(int a) => a; // Secondary

        int Baz2(int a) => a; // Noncompliant



        string Qux1(int val) // Secondary
        {
            return val.ToString();
        }

        string Qux2(int val) // Noncompliant
        {
            return val.ToString();
        }

        string Qux1(int val) => val.ToString(); // Compliant because we don't mix Body and ExpressionBody
    }
}