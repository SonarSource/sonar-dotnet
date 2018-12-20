using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo1()
//           ^^^^ Secondary
//           ^^^^ Secondary@-1
//           ^^^^ Secondary@-2
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

        void Foo4() // Noncompliant {{Update this method so that its implementation is not identical to 'Foo1'.}}
        {
            string s = "test"; // Comment are excluded from comparison
            Console.WriteLine("Result: {0}", s);
        }

        void Foo5()
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
            Console.WriteLine("different");
        }


        int DiffBySignature1(int arg1)
        {
            Console.WriteLine(arg1);
            return arg1;
        }

        string DiffBySignature2(string arg1)
        {
            Console.WriteLine(arg1);
            return arg1;
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



        int Baz1(int a) => a;

        int Baz2(int a) => a; // Compliant we ignore expression body



        string Qux1(int val)
        {
            return val.ToString();
        }

        string Qux2(int val)
        {
            return val.ToString(); // Compliant because we ignore one liner
        }

        string Qux3(int val) => val.ToString(); // Compliant we ignore expression body

        public class Foo
        {
            public void Test(string str)
            {
                Console.WriteLine(str);
            }
        }

        public class Bar
        {
            public void Test(string str)
            {
                throw new Exception(str);
            }
        }

        public static void TestFoo1(Foo x)
        {
            x.Test("hello");
            x.Test("world");
        }

        public static void TestFoo2(Foo x, string s)
        {
            x.Test("hello");
            x.Test("world");
        }

        public static void TestFoo3(string s, Foo x)
        {
            x.Test("hello");
            x.Test("world");
        }

        public static void TestBar1(Bar x)
        {
            x.Test("hello");
            x.Test("world");
        }
    }
}
