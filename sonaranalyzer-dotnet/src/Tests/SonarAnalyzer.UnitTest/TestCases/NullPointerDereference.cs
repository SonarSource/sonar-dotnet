using System;
using System.Collections.Generic;

using X = global::Tests.Diagnostics.NullPointerDereferenceWithFields;

namespace Tests.Diagnostics
{
    class NullPointerDereference
    {
        void Test_0()
        {
            int i = 0, j = 0;
            for (i = 0, j = 2; i < 2; i++)
            {
                Console.WriteLine();
            }
        }

        void Test_1(bool condition)
        {
            object o = null;
            if (condition)
            {
                M1(o.ToString()); // Noncompliant {{'o' is null on at least one execution path.}}
//                 ^
            }
            else
            {
                o = new object();
            }
            M2(o.ToString()); // Compliant
        }

        void Test_2(bool condition)
        {
            object o = new object();
            if (condition)
            {
                o = null;
            }
            else
            {
                o = new object();
            }
            M2(o.ToString()); // Noncompliant
        }

        void Test_ExtensionMethodWithNull()
        {
            object o = null;
            o.MyExtension(); // Compliant
        }

        void Test_Out()
        {
            object o1;
            object o2;
            if (OutP(out o1) &&
                OutP(out o2) &&
                o2.Count != 0)
            {
            }
        }
        void OutP(out object o) { o = new object(); }

        void Test_Struct()
        {
            int? i = null;
            if (i.HasValue) // Compliant
            { }
        }

        void Test_Foreach()
        {
            IEnumerable<int> en = null;
            foreach (var item in en) // Noncompliant
            {

            }
        }

        async System.Threading.Tasks.Task Test_Await()
        {
            System.Threading.Tasks.Task t = null;
            await t; // Noncompliant
        }

        void Test_Exception()
        {
            Exception exc = null;
            throw exc; // Noncompliant
        }

        void Test_Exception_Ok()
        {
            Exception exc = new Exception();
            throw exc;
        }

        public NullPointerDereference()
        {
            object o = null;
            Console.WriteLine(o.ToString()); // Noncompliant

            var a = new Action(() =>
            {
                object o1 = null;
                Console.WriteLine(o1.ToString()); // Noncompliant
            });
        }

        public int MyProperty
        {
            get
            {
                object o1 = null;
                Console.WriteLine(o1.ToString()); // Noncompliant
                return 42;
            }
        }

        object myObject = null;

        void Test_ConditionEqualsNull(bool condition)
        {
            object o = myObject; // can be null
            if (o == null)
            {
                M1(o.ToString()); // Noncompliant, always null
            }
            else
            {
                o = new object();
            }
            M2(o.ToString()); // Compliant
        }

        void Test_ConditionNotEqualsNull(bool condition)
        {
            object o = myObject; // can be null
            if (null != o)
            {
                M1(o.ToString()); // Compliant
            }
            else
            {
                o = new object();
            }
            M2(o.ToString()); // Compliant
        }

        void Test_Foreach_Item(bool condition)
        {
            foreach (var item in new object[0])
            {
                if (item == null)
                {
                    Console.WriteLine(item.ToString()); // Noncompliant
                }
            }
        }

        void Test_Complex(bool condition)
        {
            var item = new object();
            if (item != null && item.ToString() == "")
            {
                Console.WriteLine(item.ToString());
            }
        }

        void Constraint()
        {
            object a = GetObject();
            var b = a;
            if (a == null)
            {
                var s = b.ToString(); // Noncompliant
            }
        }

        void Equals(object b)
        {
            object a = null;
            if (a == b)
            {
                b.ToString(); // Noncompliant
            }
            else
            {
                b.ToString();
            }

            a = new object();
            if (a == b)
            {
                b.ToString();
            }
            else
            {
                b.ToString();
            }
        }

        void NotEquals(object b)
        {
            object a = null;
            if (a != b)
            {
                b.ToString();
            }
            else
            {
                b.ToString(); // Noncompliant
            }

            a = new object();
            if (a != b)
            {
                b.ToString();
            }
            else
            {
                b.ToString();
            }
        }

        void ElementAccess(int[,] arr)
        {
            if (arr == null)
            {
                Console.WriteLine(arr[10, 10]); // Noncompliant
            }
            else
            {
                Console.WriteLine(arr[10, 10]);
            }
        }

        void NullableGetTypeCall()
        {
            int? i = null;
            var v = i.HasValue;
            var s = i.ToString();

            i = 5;
            var t = i.GetType();

            i = null;
            var t = i.GetType(); // Noncompliant
        }

        static void MultiplePop()
        {
            MyClass o = null;
            o = new MyClass
            {
                MyProperty = ""
            };
            o.ToString(); // Compliant
        }

        class MyClass
        {
            public string MyProperty { get; set; }
        }

        public void Assert1(object o1)
        {
            System.Diagnostics.Debug.Assert(o1 != null);
            o1.ToString(); // Compliant
            System.Diagnostics.Debug.Assert(o1 == null);
            o1.ToString(); // Compliant
        }

        public void Assert2(object o1)
        {
            System.Diagnostics.Debug.Assert(o1 == null);
            o1.ToString(); // Compliant, we don't learn on Assert
        }
    }

    class A
    {
        protected object _bar;
    }

    class B
    {
        public const string Whatever = null;
    }

    class NullPointerDereferenceWithFields : A
    {
        private object _foo1;
        protected object _foo2;
        internal object _foo3;
        public object _foo4;
        protected internal object _foo5;
        private readonly object _foo7 = new object();
        private static object _foo8;
        private const object NullConst = null;
        private readonly object NullReadOnly = null;

        void DoNotLearnFromReadonly()
        {
            NullReadOnly.ToString(); // Compliant. TODO: SLVS-1140
        }

        void DoLearnFromConstants()
        {
            NullConst.ToString(); // Noncompliant
//          ^^^^^^^^^
        }

        void DoLearnFromAnyConstant1()
        {
            NullConst.ToString(); // Noncompliant
        }
        void DoLearnFromAnyConstant2()
        {
            NullPointerDereferenceWithFields.NullConst.ToString(); // Noncompliant
        }
        void DoLearnFromAnyConstant3()
        {
            Tests.Diagnostics.NullPointerDereferenceWithFields.NullConst.ToString(); // Noncompliant
        }
        void DoLearnFromAnyConstant4()
        {
            X.NullConst.ToString(); // Noncompliant
        }
        void DoLearnFromAnyConstant5()
        {
            B.Whatever.ToString(); // Noncompliant
        }

        void DumbestTestOnFoo1()
        {
            object o = null;
            _foo1 = o;
            _foo1.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo2()
        {
            object o = null;
            _foo2 = o;
            _foo2.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo3()
        {
            object o = null;
            _foo3 = o;
            _foo3.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo4()
        {
            object o = null;
            _foo4 = o;
            _foo4.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo5()
        {
            object o = null;
            _foo5 = o;
            _foo5.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo8()
        {
            object o = null;
            _foo8 = o;
            _foo8.ToString(); // Noncompliant
//          ^^^^^
        }
        void DumbestTestOnFoo6()
        {
            _foo6.ToString(); // compliant
        }
        void DumbestTestOnFoo7()
        {
            _foo7.ToString(); // compliant
        }

        void DifferentFieldAccess1()
        {
            object o = null;
            this._foo1 = o;
            this._foo1.ToString(); // Noncompliant
        }
        void DifferentFieldAccess2()
        {
            this._foo1 = null;
            _foo1.ToString(); // Noncompliant
        }
        void DifferentFieldAccess3()
        {
            _foo1 = null;
            this._foo1.ToString(); // Noncompliant
        }
        void DifferentFieldAccess4()
        {
            _foo1 = null;
            (((this)))._foo1.ToString(); // Noncompliant
        }
        void DifferentFieldAccess5()
        {
            _foo1 = null;
            (((this._foo1))).ToString(); // Noncompliant
        }

        void OtherInstanceFieldAccess()
        {
            object o = null;
            var other = new NullPointerDereferenceWithFields();
            other._foo1 = o;
            other._foo1.ToString(); // Compliant
        }
        void OtherInstanceFieldAccess2()
        {
            object o = null;
            _foo1 = o;
            (new NullPointerDereferenceWithFields())._foo1.ToString(); // Compliant
        }

        void ParenthesizedAccess1()
        {
            object o = null;
            _foo1 = o;
            (_foo1).ToString(); // Noncompliant
        }
        void ParenthesizedAccess2()
        {
            object o = null;
            ((((((this)))._foo1))) = o;
            (((_foo1))).ToString(); // Noncompliant
        }

        void VariableFromField()
        {
            _foo1 = null;
            var o = _foo1;
            o.ToString(); // Noncompliant
//          ^
        }

        void LearntConstraintsOnField()
        {
            if (_foo1 == null)
            {
                _foo1.ToString(); // Noncompliant
//              ^^^^^
            }
        }

        void LearntConstraintsOnBaseField()
        {
            if (_bar == null)
            {
                _bar.ToString(); // Noncompliant
//              ^^^^
            }
        }

        void LearntConstraintsOnFieldAssignedToVar()
        {
            if (_foo1 == null)
            {
                var o = _foo1;
                o.ToString(); // Noncompliant
//              ^
            }
        }

        void LambdaConstraint()
        {
            _foo1 = null;
            var a = new Action(() =>
            {
                _foo1.ToString(); // Compliant
            });
            a();
        }

        void Assert1()
        {
            System.Diagnostics.Debug.Assert(_foo1 != null);
            _foo1.ToString(); // Compliant
            System.Diagnostics.Debug.Assert(_foo1 == null);
            _foo1.ToString(); // Compliant
        }

        void CallToExtensionMethodsShouldNotRaise()
        {
            object o = null;
            _foo1 = o;
            _foo1.MyExtension(); // Compliant
        }

        void CallToMethodsShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            (((this))).DoSomething();
            _foo1.ToString(); // Compliant
        }

        void CallToExtensionMethodsShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            this.MyExtension();
            _foo1.ToString(); // Compliant
        }

        void CallToNameOfShouldNotResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            var name = nameof(DoSomething);
            _foo1.ToString(); // Noncompliant
        }

        void DoSomething() { }

        void TestNameOf()
        {
            var x = nameof();
            var y = nameof(a, b);
            x.ToString();
        }

        string TryCatch1()
        {
            object o = null;
            try
            {
                o = new object();
            }
            catch
            {
                o = new object();
            }
            return o.ToString();
        }

        string TryCatch2()
        {
            object o = null;
            try
            {
                o = new object();
            }
            catch (Exception)
            {
                o = new object();
            }
            return o.ToString();
        }

        string TryCatch3()
        {
            object o = null;
            try
            {
                o = new object();
            }
            catch (ApplicationException)
            {
                o = new object();
            }
            return o.ToString();
        }
    }

    static class Extensions
    {
        public static void MyExtension(this object o) { }
    }

    class Foo // https://github.com/SonarSource/sonar-csharp/issues/538
    {
        private string bar;

        private void Invoke()
        {
            this.bar = null;
            if (this.bar != null)
                this.bar.GetHashCode();
        }
    }

}