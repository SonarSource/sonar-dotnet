﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public void M1(string s) { }
        public void M2(string s) { }

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
                o2.ToString() != "")
            {
            }
        }
        bool OutP(out object o) { o = new object(); return true; }

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

        object GetObject() => null;

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
            var t2 = i.GetType(); // Noncompliant
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

        public void StringEmpty(string s1)
        {
            if (string.IsNullOrEmpty(s1))
            {
                s1.ToString(); // Noncompliant
            }
            else
            {
                s1.ToString(); // Compliant
            }
        }

        public void StringEmpty1(string s1)
        {
            if (s1 == "" || s1 == null)
            {
                s1.ToString(); // Noncompliant
            }
            else
            {
                s1.ToString(); // Compliant
            }
        }

        void StringEmpty3(string path)
        {
            var s = path == "" ? new string[] { } : path.Split('/');
        }

        void StringEmpty4(string path)
        {
            var s = path == null ? new string[] { } : path.Split('/');
        }

        void StringEmpty5(string path)
        {
            var s = path == null ? path.Split('/') : new string[] { }; // Noncompliant
        }

        void StringEmpty6(string path)
        {
            var s = string.IsNullOrEmpty(path) ? path.Split('/') : new string[] { }; // Noncompliant
        }

        void StringEmpty7(string path)
        {
            var s = string.IsNullOrEmpty(path) ? new string[] { } : path.Split('/');
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
        object _foo6;
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

        void CallToStaticMethodsShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            Console.WriteLine(); // This particular method has no side effects
            _foo1.ToString(); // Compliant, False Negative
            o.ToString(); // Noncompliant, local variable constraints are not cleared
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/947
        void CallToMonitorWaitShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            System.Threading.Monitor.Wait(this); // This is a multi-threaded application, the fields could change
            _foo1.ToString(); // Compliant
            o.ToString(); // Noncompliant, local variable constraints are not cleared
        }

        void CallToNameOfShouldNotResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            var name = nameof(DoSomething);
            _foo1.ToString(); // Noncompliant
        }

        void DereferenceInNameOfShouldNotRaise()
        {
            object o = null;
            var name = nameof(o.ToString);
        }

        void DoSomething() { }

        void TestNameOf(int a)
        {
            var x = nameof(a);
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

    class Foo // https://github.com/SonarSource/sonar-dotnet/issues/538
    {
        private string bar;

        private void Invoke()
        {
            this.bar = null;
            if (this.bar != null)
                this.bar.GetHashCode();
        }
    }

    public static class GuardedTests
    {
        public static void Guarded(string s1)
        {
            Guard1(s1);

            if (s1 == null) s1.ToUpper(); // Compliant, this code is unreachable
        }

        public static void Guard1<T>([ValidatedNotNull]T value) where T : class { }

        // https://github.com/SonarSource/sonar-dotnet/issues/3850
        public static void UsedAsExtension()
        {
            object a = null;
            if (a.IsNotNull())
            {
                a.ToString();
            }
        }

        public static void UsedAsMethod()
        {
            object a = null;
            if (IsNotNull(a))
            {
                a.ToString();   // Compliant
            }
        }

        public static bool IsNotNull([ValidatedNotNull] this object item) => item != null;

        [AttributeUsage(AttributeTargets.Parameter)]
        public sealed class ValidatedNotNullAttribute : Attribute { }
    }

    class AsyncAwait
    {
        string x;
        async Task Foo(Task t)
        {
            string s = null;
            x = s;
            await t; // awaiting clears the constraints
            x.ToString(); // Compliant
            s.ToString(); // Noncompliant
        }
    }

    class TestLoopWithBreak
    {
        public static void LoopWithBreak(IEnumerable<string> list)
        {
            foreach (string x in list)
            {
                try
                {
                    if (x == null)
                    {
                        x.ToString(); // Noncompliant
                    }
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        public static IEnumerable<string> LoopWithYieldBreak(IEnumerable<string> list)
        {
            foreach (string x in list)
            {
                try
                {
                    if (x == null)
                    {
                        yield return x.ToString(); // Noncompliant
                    }
                    yield break;
                }
                finally
                {
                    // do stuff
                }
            }
        }

    }

    // see https://github.com/SonarSource/sonar-dotnet/issues/890
    class TestForLoop
    {
        string Foo()
        {
            string s = null;
            for (int i = 0; i < 10; i++)
            {
                s = "";
            }
            return s.Trim(); // Noncompliant FP due to loop traversal
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3156
    class ForEachCollection
    {
        string DoSomething(IEnumerable<object> list, object current)
        {
            if (current == null)
            {
                //SE creates both constrains for 'current'
            }
            foreach(var item in list)
            {
                if (item == current)
                {
                    return item.ToString(); // Noncompliant, null constraint is inherited from 'current == null' check. If list would contain a "null" item, it's a TP.
                }
                else if (item.ToString() == "xxx") // 'item' does not contain constraints by default, issue is not raised here anyway
                {
                    return item.ToString();
                }
            }
            return null;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3290
    class Linq_OrDefault
    {
        // All XxxOrDefault Linq extensions should create both null and not-null constraints.
        string DoSomething(IEnumerable<object> list)
        {
            var item = list.FirstOrDefault();
            return item.ToString(); // False Negative, item could be null here
        }

        string DoSomethingArg(IEnumerable<object> list)
        {
            var item = list.SingleOrDefault(x => x != null);
            return item.ToString(); // False Negative, item could be null here
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4537
    class Repro_4537
    {
        private void ConditionalAccess_NullCoalescing()
        {
            string someString = null;

            if (!someString?.Contains("a") ?? true)
                Console.WriteLine("It's null or doesn't contain 'a'");
            else
                Console.WriteLine(someString.Length); // Noncompliant FP, this path is unreachable
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3395
namespace Repro_3395
{
    public enum Helper
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P
    }
    public static class Test
    {
        public static void SupportedSize()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A | helper == Helper.B | helper == Helper.C | helper == Helper.D
                | helper == Helper.E | helper == Helper.F | helper == Helper.G | helper == Helper.H)
            {
                o.ToString(); // Noncompliant, this condition size is within the limit
            }
        }

        public static void UnsupportedSize()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A | helper == Helper.B | helper == Helper.C | helper == Helper.D
                | helper == Helper.E | helper == Helper.F | helper == Helper.G | helper == Helper.H
                | helper == Helper.I)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }

        public static void OrConstraint()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A | helper == Helper.B | helper == Helper.C | helper == Helper.D
                | helper == Helper.E | helper == Helper.F | helper == Helper.G | helper == Helper.H
                | helper == Helper.I | helper == Helper.A | helper == Helper.B | helper == Helper.C
                | helper == Helper.D | helper == Helper.E | helper == Helper.F | helper == Helper.G
                | helper == Helper.H | helper == Helper.I | helper == Helper.A | helper == Helper.B
                | helper == Helper.C | helper == Helper.D | helper == Helper.E | helper == Helper.F
                | helper == Helper.G | helper == Helper.H | helper == Helper.I | helper == Helper.J)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }

        public static void AndConstraint()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A & helper == Helper.B & helper == Helper.C & helper == Helper.D
                & helper == Helper.E & helper == Helper.F & helper == Helper.G & helper == Helper.H
                & helper == Helper.I & helper == Helper.A & helper == Helper.B & helper == Helper.C
                & helper == Helper.D & helper == Helper.E & helper == Helper.F & helper == Helper.G
                & helper == Helper.H & helper == Helper.I & helper == Helper.A & helper == Helper.B
                & helper == Helper.C & helper == Helper.D & helper == Helper.E & helper == Helper.F
                & helper == Helper.G & helper == Helper.H & helper == Helper.I & helper == Helper.J)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }

        public static void XorConstraint()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A ^ helper == Helper.B ^ helper == Helper.C ^ helper == Helper.D
                ^ helper == Helper.E ^ helper == Helper.F ^ helper == Helper.G ^ helper == Helper.H
                ^ helper == Helper.I ^ helper == Helper.A ^ helper == Helper.B ^ helper == Helper.C
                ^ helper == Helper.D ^ helper == Helper.E ^ helper == Helper.F ^ helper == Helper.G
                ^ helper == Helper.H ^ helper == Helper.I ^ helper == Helper.A ^ helper == Helper.B
                ^ helper == Helper.C ^ helper == Helper.D ^ helper == Helper.E ^ helper == Helper.F
                ^ helper == Helper.G ^ helper == Helper.H ^ helper == Helper.I ^ helper == Helper.J)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }

        public static void ComparisonConstraint()
        {
            var helper = Helper.A;
            object o = null;
            if (helper > Helper.A | helper > Helper.B | helper > Helper.C | helper > Helper.D
                | helper >= Helper.E | helper >= Helper.F | helper >= Helper.G | helper >= Helper.H
                | helper == Helper.I | helper == Helper.A | helper == Helper.B | helper == Helper.C
                | helper < Helper.D | helper < Helper.E | helper < Helper.F | helper < Helper.G
                | helper >= Helper.H | helper >= Helper.I | helper >= Helper.A | helper >= Helper.B
                | helper != Helper.C | helper != Helper.D | helper != Helper.E | helper != Helper.F
                | helper == Helper.G | helper == Helper.H | helper == Helper.I | helper == Helper.J)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }

        public static void MixedConstraints()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A & helper == Helper.B ^ helper == Helper.C & helper == Helper.D
                | helper == Helper.E & helper == Helper.F ^ helper == Helper.G & helper == Helper.H
                | helper == Helper.I & helper == Helper.A ^ helper == Helper.B & helper == Helper.C
                | helper == Helper.D & helper == Helper.E ^ helper == Helper.F & helper == Helper.G
                | helper == Helper.H & helper == Helper.I ^ helper == Helper.A & helper == Helper.B
                | helper == Helper.C & helper == Helper.D ^ helper == Helper.E & helper == Helper.F
                | helper == Helper.G & helper == Helper.H ^ helper == Helper.I & helper == Helper.J)
            {
                o.ToString(); // FN, the condition state generation is too big to explore all constraint combinations
            }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4784
    class Repro_4784
    {
        public static int Reproducer()
        {
            List<long> test = new[] { 1L, 2L, 3L }.ToList();
            if (test?.Count == 0)
            {
                // Do something
            }

            return test.Count; // Noncompliant FP
        }

        public static int NoIssueReported()
        {
            var something = new Something();
            if (something?.SomeProperty == 0)
            {
                // Do something
            }

            return something.SomeProperty;
        }

        public static int IssueReported()
        {
            var something = GetSomething();
            if (something?.SomeProperty == 0)
            {
                // Do something
            }

            return something.SomeProperty; // Noncompliant
        }

        public static Something GetSomething()
        {
            return new Something();
        }

        public class Something
        {
            public int SomeProperty { get; set; }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4989
    class Repro_4989
    {
        static void Main(string[] args)
        {
            var providerCourses = new List<ProviderCourse>
            {
                new ProviderCourse
                {
                    Items = new List<string>
                    {
                        "item1",
                        "item2"
                    }
                },
                new ProviderCourse
                {
                    Items = new List<string>
                    {
                        "item1",
                        "item2"
                    }
                }
            };

            foreach (var providerCourse in providerCourses)
            {
                if (!providerCourse?.Items?.Any() ?? true)
                {
                    Console.WriteLine("FAIL");
                    continue;
                }

                var _ = providerCourse.Items.Where(items => items == "item1"); // Noncompliant FP
            }
        }

        class ProviderCourse
        {
            public IEnumerable<string> Items { get; set; }
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/5285
public class Repro_5289
{
    public void A(ref string s)
    {
        s = "not null";
    }

    public void B(int i)
    {
        // empty
    }

    public void C(double[] a)
    {
        if (a == null)
        {
            B(0);
        }

        string s = null;
        A(ref s);

        if (a != null)
        {
            B(a.Length); // Noncompliant FP, the unrelated "A(ref s)" call breaks constraint on "a" variable
        }
    }
}

namespace ValidatedNotNullAttributeTest
{
    public sealed class ValidatedNotNullAttribute : Attribute { }

    public static class Guard
    {
        public static void NotNull<T>([ValidatedNotNullAttribute] this T value, string name) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }
    }

    public static class Utils
    {
        public static string ToUpper(string value)
        {
            Guard.NotNull(value, nameof(value));
            if (value != null)
            {
                return value.ToUpper(); // Compliant
            }
            return value.ToUpper(); // Compliant
        }
    }
}
