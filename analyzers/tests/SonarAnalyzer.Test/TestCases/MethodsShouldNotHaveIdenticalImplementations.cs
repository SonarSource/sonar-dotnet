using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Foo1()
//           ^^^^ Secondary {{Update this method so that its implementation is not identical to 'Foo2'.}}
//           ^^^^ Secondary@-1 {{Update this method so that its implementation is not identical to 'Foo3'.}}
//           ^^^^ Secondary@-2 {{Update this method so that its implementation is not identical to 'Foo4'.}}
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

        void Foo1(string arg1)
        {
            string s = arg1;
            Console.WriteLine("Result: {0}", s);
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

        static string M(int x)
        {
            x += 1;
            return x.ToString();
        }

        public void Method()
        {
            static string LocalFunction(int x)
//                        ^^^^^^^^^^^^^ Secondary
//                        ^^^^^^^^^^^^^ Secondary@-1
            {
                x += 42;
                return x.ToString();
            }

            static string LocalFunctionCopy(int x) // Noncompliant
            {
                x += 42;
                return x.ToString();
            }
        }

        public string MethodWhichCopiesLocalFunction(int x) // Noncompliant
        {
            x += 42;
            return x.ToString();
        }
    }

    struct SomeStruct
    {
        void Foo1() // Secondary
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        void Foo2() // Noncompliant
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }
    }

    public abstract class InheritedConstraints
    {
        public class Item { public void Action() { } }
        public class Mobile { public void Action() { } }

        public abstract void WriteItem<T>(T item) where T : Item;
        public abstract void WriteItem2<T>(T item) where T : Item;
        public abstract void WriteMobile<T>(T item) where T : Mobile;

        public class Derived : InheritedConstraints
        {
            public override void WriteItem<T>(T item) // Secondary
            {
                item.Action();
                Console.WriteLine("One");
                Console.WriteLine("Two");
            }

            public override void WriteItem2<T>(T item) // Noncompliant. The WriteItem methods have the same inherited constraints.
            {
                item.Action();
                Console.WriteLine("One");
                Console.WriteLine("Two");
            }

            public override void WriteMobile<T>(T item) // Compliant. The WriteMobile method has a different inherited constraint.
            {
                item.Action();
                Console.WriteLine("One");
                Console.WriteLine("Two");
            }
        }
    }

    // https://sonarsource.atlassian.net/browse/NET-348
    class Repro_348
    {
        T Method1<T>() => default;  // FN
        T Method2<T>() => default;
    }
}
