using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class RedundantDeclaration
    {
        public RedundantDeclaration()
        {
            MyEvent += new EventHandler(Handle); // Fixed
            MyEvent += (a, b) => { Handle(a, b); };

            (new EventHandler(Handle))(1, null);

            MyEvent2 = new MyMethod(Handle); // Fixed
            MyEvent2 = new MyMethod(            // Fixed
                delegate (int i, int j) { });   // Fixed

            Delegate anotherEvent = new EventHandler(Handle);
            BoolDelegate myDelegate = new BoolDelegate(() => true);      // Fixed

            MyEvent2 = delegate (int i, int j) { Console.WriteLine(); }; // Fixed
            MyEvent = delegate { Console.WriteLine("fdsfs"); };

            var l = new List<int>() { }; // Fixed
            l = new List<int>();
            var o = new object() { }; // Fixed
            o = new object { };

            var ints = new int[] { 1, 2, 3 }; // Fixed
            ints = new[] { 1, 2, 3 };
            ints = new int[] { 1, 2, 3 }; // Fixed
            ints = new int[] { };

            var ddd = new double[] { 1, 2, 3.0 }; // Compliant the element types are not the same as the specified one

            var xxx = new int[,] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };
            var yyy = new int[,] { { 1, 1, 1 }, { 2, 2, 2 }, { 3, 3, 3 } };

            // see https://github.com/SonarSource/sonar-dotnet/issues/1840
            var multiDimIntArray = new int[][] // Compliant - type specifier is mandatory here
            {
                new int[] { 1 } // Fixed
            };
            var zzz = new int[][] { new[] { 1, 2, 3 }, new int[0], new int[0] };
            var www = new int[][][] { new[] { new[] { 0 } } };

            int? xx = ((new int?(5))); // Fixed
            xx = new Nullable<int>(5); // Fixed
            var rr = new int?(5);

            NullableTest1(new int?(5));
            NullableTest1<int>(new int?(5)); // Fixed
            NullableTest2(new int?(5)); // Fixed

            Func<int, int?> f = new Func<int, int?>(// Fixed
                i => new int?(i)); // Fixed
            f = i =>
            {
                return new int?(i); // Fixed
            };

            Delegate d = new Action(() => { });
            Delegate d2 = new Func<double>(() => { return 1; });

            NullableTest2(f(5));

            var f2 = new Func<int, int?>(i => i);

            Func<int, int> f1 = (int i) => i; //Fixed
            Func<int, int> f3 = (i) => i;
            var transformer = Funcify((string x) => new { Original = x, Normalized = x.ToLower() });
            var transformer2 = Funcify2((string x) => new { Original = x, Normalized = x.ToLower() }); // Fixed

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
            return (new BoolDelegate(() => true));        // Fixed
        }

        public Func<BoolDelegate> P()
        {
            return (() => new BoolDelegate(() => true));  // Fixed
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
