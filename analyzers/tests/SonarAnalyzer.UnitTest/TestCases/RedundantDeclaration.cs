using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public RedundantDeclaration()
        {
            MyEvent += new EventHandler(Handle); // Noncompliant {{Remove the explicit delegate creation; it is redundant.}}
//                     ^^^^^^^^^^^^^^^^
            MyEvent += (a, b) => { Handle(a, b); };

            (new EventHandler(Handle))(1, null);

            MyEvent2 = new MyMethod(Handle); // Noncompliant
            MyEvent2 = new MyMethod(            // Noncompliant
                delegate (int i, int j) { });   // Noncompliant {{Remove the parameter list; it is redundant.}}
//                       ^^^^^^^^^^^^^^

            Delegate anotherEvent = new EventHandler(Handle);
            BoolDelegate myDelegate = new BoolDelegate(() => true);      // Noncompliant {{Remove the explicit delegate creation; it is redundant.}}

            MyEvent2 = delegate (int i, int j) { Console.WriteLine(); }; // Noncompliant
            MyEvent = delegate { Console.WriteLine("fdsfs"); };

            var l = new List<int>() { }; // Noncompliant {{Remove the initializer; it is redundant.}}
//                                  ^^^
            l = new List<int>();
            var o = new object() { }; // Noncompliant
//                               ^^^
            o = new object { };

            var ints = new int[] { 1, 2, 3 }; // Noncompliant {{Remove the array type; it is redundant.}}
//                         ^^^
            ints = new[] { 1, 2, 3 };
            ints = new int[3] { 1, 2, 3 }; // Noncompliant {{Remove the array size specification; it is redundant.}}
//                         ^
            ints = new int[] { };

            var ddd = new double[] { 1, 2, 3.0 }; // Compliant the element types are not the same as the specified one

            var xxx = new int[,] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };
            var yyy = new int[3 // Noncompliant, we report two issues on this to keep the comma unfaded
                , 3// Noncompliant
                ] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };

            // see https://github.com/SonarSource/sonar-dotnet/issues/1840
            var multiDimIntArray = new int[][] // Compliant - type specifier is mandatory here
            {
                new int[] { 1 } // Noncompliant
            };
            var zzz = new int[][] { new[] { 1, 2, 3 }, new int[0], new int[0] };
            var www = new int[][][] { new[] { new[] { 0 } } };

            int? xx = ((new int?(5))); // Noncompliant {{Remove the explicit nullable type creation; it is redundant.}}
//                      ^^^^^^^^
            xx = new Nullable<int>(5); // Noncompliant
            var rr = new int?(5);

            NullableTest1(new int?(5));
            NullableTest1<int>(new int?(5)); // Noncompliant
            NullableTest2(new int?(5)); // Noncompliant

            Func<int, int?> f = new Func<int, int?>(// Noncompliant
                i => new int?(i)); // Noncompliant
            f = i =>
            {
                return new int?(i); // Noncompliant
            };

            Delegate d = new Action(() => { });
            Delegate d2 = new Func<double>(() => { return 1; });

            NullableTest2(f(5));

            var f2 = new Func<int, int?>(i => i);

            Func<int, int> f1 = (int i) => i; //Noncompliant {{Remove the type specification; it is redundant.}}
//                               ^^^
            Func<int, int> f3 = (i) => i;
            var transformer = Funcify((string x) => new { Original = x, Normalized = x.ToLower() });
            var transformer2 = Funcify2((string x) => new { Original = x, Normalized = x.ToLower() }); // Noncompliant

            RefDelegateMethod((ref int i) => { i++; });
        }

        public void M()
        {
            dynamic d = new object();
            Test(d, new BoolDelegate(() => true)); // Special case, d is dynamic
            Test2(null, new BoolDelegate(() => true)); // Compliant
        }

        public Delegate N()
        {
            Delegate foo;
            foo = (new BoolDelegate(() => true));
            return (new BoolDelegate(() => true));
        }

        public BoolDelegate O()
        {
            return (new BoolDelegate(() => true));        // Noncompliant {{Remove the explicit delegate creation; it is redundant.}}
        }

        public Func<BoolDelegate> P()
        {
            return (() => new BoolDelegate(() => true));  // Noncompliant {{Remove the explicit delegate creation; it is redundant.}}
        }

        public abstract void Test(object o, BoolDelegate f);
        public abstract void Test2(object o, Delegate f);
        public delegate bool BoolDelegate();

        private event EventHandler MyEvent;
        private event MyMethod MyEvent2;

        private delegate void MyMethod(int i, int j);
        private delegate void MyMethod2(ref int i);
        private void RefDelegateMethod(MyMethod2 f) { }

        public static Func<T, TResult> Funcify<T, TResult>(Func<T, TResult> f) { return f; }
        public static Func<string, TResult> Funcify2<TResult>(Func<string, TResult> f) { return f; }

        public static void NullableTest1<T>(T? x) where T : struct { }
        public static void NullableTest2(int? x) { }

        private void Handle(object sender, EventArgs e) {}
        private void Handle(int i, int j) {}
    }
}
