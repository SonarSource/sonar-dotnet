using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        void Test_Property()
        {
            MyClass o = null;
            _ = o.Property;   // Noncompliant
            o = null;
            o.Property = "";  // Noncompliant
        }

        void Test_Field()
        {
            MyClass o = null;
            _ = o.Field;  // Noncompliant
            o = null;
            o.Field = ""; // Noncompliant
        }

        void Test_Event()
        {
            MyClass o = null;
            o.Event += (s, e) => throw new NotImplementedException(); // Noncompliant
            o = null;
            o.Event -= (s, e) => throw new NotImplementedException(); // Noncompliant
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

        void Test_NullableValueTypes<T>(T? arg) where T : struct
        {
            int? i = null;
            i.GetType();            // Noncompliant
            i = 42;
            i.GetType();            // Compliant
            i = null;
            _ = i.HasValue;         // Compliant - safe to call
            i = null;
            _ = i.Value;            // Compliant - handled by rule S3655
            i = null;
            _ = (int)i;             // Compliant - handled by rule S3655
            i = null;
            i.GetValueOrDefault();  // Compliant - safe to call
            i = null;
            i.Equals(null);         // Compliant - safe to call
            i = null;
            i.GetHashCode();        // Compliant - safe to call
            i = null;
            i.ToString();           // Compliant - safe to call

            arg.GetType();          // Compliant
            arg = null;
            arg.GetType();          // Noncompliant

            T? localNotNull = new T();
            localNotNull.GetType();             // Compliant
            T? localNull = null;
            localNull.GetType();                // Noncompliant
            T? localNewNull = new T?();
            localNewNull.GetType();             // Noncompliant
            T? localDefaultT = default(T);
            localDefaultT.GetType();            // Compliant
            T? localDefaultNullableT = default(T?);
            localDefaultNullableT.GetType();    // Noncompliant
        }

        class HasGenericNullable<T> where T : struct
        {
            public T? Property { get; set; }

            public void M()
            {
                Property = null;
                _ = Property.HasValue;  // Compliant
                Property = null;
                Property.GetType();     // Noncompliant
                Property = default(T);
                Property.GetType();     // Compliant
            }
        }

        const int constInt = 42;
        const string constNullString = null;
        const string constNotNullString = "const text";
        const object constNullObject = null;

        void Const()
        {
            constInt.GetType();             // Compliant

            constNullString.ToString();     // Noncompliant
            constNullString.ToString();     // Compliant - can only be reached when not null

            constNotNullString.ToString();  // Compliant

            constNullObject.ToString();     // Noncompliant
            constNullObject.ToString();     // Compliant - can only be reached when not null
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
            throw exc; // FN, was supported in the old engine. Throw is a branch in Roslyn CFG.
        }

        void Test_Exception_Ok()
        {
            Exception exc = new Exception();
            throw exc;
        }

        void Test_IndexerElementAccess()
        {
            List<int> list = null;
            var i = list[0];   // Noncompliant
            //      ^^^^
        }

        void Test_ArrayElementAccess()
        {
            int[] arr = null;
            var i = arr[0];   // Noncompliant
            //      ^^^
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
                var s = b.ToString(); // FN, was supported in the old engine. Requires relations from MMF-2563
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

        static void MultiplePop()
        {
            MyClass o = null;
            o = new MyClass
            {
                Property = ""
            };
            o.ToString(); // Compliant
        }

        class MyClass
        {
            public string Property { get; set; }
            public string Field;
            public event EventHandler Event;

            public void M()
            {
                Property = null;
                Property.ToString();  // Noncompliant
            }
        }

        public void Assert1(object o)
        {
            System.Diagnostics.Debug.Assert(o != null);
            o.ToString(); // Compliant
            System.Diagnostics.Debug.Assert(o == null);
            o.ToString(); // Compliant, because we already know that o is not null from o.ToString()
        }

        public void Assert2(object o1)
        {
            System.Diagnostics.Debug.Assert(o1 == null);
            o1.ToString(); // Noncompliant FP
        }

        public void LearnFromArguments(object o)
        {
            LearnFromArguments(o == null);
            o.ToString(); // Noncompliant
        }

        public void DebugFail()
        {
            object o = null;
            System.Diagnostics.Debug.Fail("Fail");
            o.ToString(); // Compliant, unreachable
        }

        public void DebugFail_TryCatchFinally()
        {
            object o = null;
            try
            {
                System.Diagnostics.Debug.Fail("Fail");
                o.ToString(); // Compliant, unreachable
            }
            catch
            {
                o.ToString(); // Noncompliant
            }
            finally
            {
                o.ToString(); // Noncompliant
            }
        }

        public void EnvironmentExit()
        {
            object o = null;
            Environment.Exit(-1);
            o.ToString(); // Compliant, unreachable
        }

        public void EnvironmentFailFast()
        {
            object o = null;
            Environment.FailFast("Fail");
            o.ToString(); // Compliant, unreachable
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

        public void StringIsNullOrWhiteSpace(string s1)
        {
            if (string.IsNullOrWhiteSpace(s1))
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

        void BinaryOrLeft(int arg)
        {
            object o = null;
            if (o == null | arg == 0)
            {
                o.ToString();   // Noncompliant
            }
            else
            {
                o.ToString();   // Compliant, unreachable
            }
        }

        void BinaryOrRight(int arg)
        {
            object o = null;
            if (arg == 0 | o == null)
            {
                o.ToString();   // Noncompliant
            }
            else
            {
                o.ToString();   // Compliant, unreachable
            }
        }

        void ShortCircuitOrLeft(int arg)
        {
            object o = null;
            if (o == null || arg == 0)
            {
                o.ToString();   // Noncompliant
            }
            else
            {
                o.ToString();   // Compliant, unreachable
            }
        }

        void ShortCircuitOrRight(int arg)
        {
            object o = null;
            if (arg == 0 || o == null)
            {
                o.ToString();   // Noncompliant
            }
            else
            {
                o.ToString();   // Compliant, unreachable
            }
        }

        void BinaryAndLeft(int arg)
        {
            object o = null;
            var isFalse = false;
            if (isFalse & arg == 0)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        void BinaryAndRight(int arg)
        {
            object o = null;
            var isFalse = false;
            if (arg == 0 & isFalse)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        void ShortCircuitAndLeft(int arg)
        {
            object o = null;
            var isFalse = false;
            if (isFalse && arg == 0)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        void ShortCircuitAndRight(int arg)
        {
            object o = null;
            var isFalse = false;
            if (arg == 0 && isFalse)
            {
                o.ToString();   // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        void ShortCircuit()
        {
            object o = null;
            var isFalse = false;
            if (isFalse && o.ToString() == null)    // Compliant, unreachable
            {
                o.ToString();    // Compliant, unreachable
            }
            else
            {
                o.ToString();   // Noncompliant
            }
        }

        void ShortCircuit_Reachable()
        {
            object o = null;
            var isTrue = true;
            if (isTrue && o.ToString() == null)    // Noncompliant
            {
                o.ToString();   // Compliant, we've learned that o cannot be null on this path from calling o.ToString();
            }
            else
            {
                o.ToString();   // Compliant for the same reason
            }
        }

        public void Type_IsAssignableFrom()
        {
            var other = typeof(object);
            var type = typeof(NullPointerDereference);
            if (other.IsAssignableFrom(type))
            {
                type.ToString();
            }
            else
            {
                type.ToString();    // Compliant, type was not null
            }
        }

        public void Type_IsAssignableFrom(Type type)
        {
            var other = typeof(object);
            if (other.IsAssignableFrom(type))
            {
                type.ToString();
            }
            else
            {
                type.ToString();    // Compliant, .NET Fx doesn't have NotNullWhen(true) annotation, we don't learn Null from the .NET version
            }
        }

        public void TypeOf()
        {
            Type type = null;
            type = typeof(object);
            type.ToString(); // Compliant
        }

        public void WithDelegate(object asParameter, object asVariable)
        {
            if (asParameter == null)
            {
            }

            if (asVariable == null)
            {
            }

            SomeAction(asParameter.ToString);       // Noncompliant
            Func<string> f = asVariable.ToString;   // Noncompliant

            void SomeAction(Func<string> a)
            { }
        }

        public void ConcatNulls()
        {
            string s = null;
            s = s + null;
            s.ToString();   // Compliant
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
        private const string NullConstString = null;
        private const string NotNullConst = "Lorem ipsum";
        private readonly object NullReadOnly = null;

        void DoNotLearnFromReadonly()
        {
            NullReadOnly.ToString(); // Compliant, we don't detect reassignments in constructor(s)
        }

        void DoLearnFromConstants()
        {
            NotNullConst.ToString();    // Compliant
            NullConstString.ToString(); // Noncompliant
            NullConst.ToString();       // Noncompliant
//          ^^^^^^^^^
        }

        void DoLearnFromAnyConstant1()
        {
            NullConst.ToString();       // Noncompliant
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
            _foo1 = null;
            (new NullPointerDereferenceWithFields())._foo1.ToString(); // Compliant, while _foo1 is null here, we would need to inspect the constructor to recognize that
        }
        void OtherInstanceFieldAccess3()
        {
            _foo1 = new object();
            (new NullPointerDereferenceWithFields())._foo1.ToString(); // Compliant, while _foo1 is null here, we would need to inspect the constructor to recognize that
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

        void ActionInvocation(bool condition)
        {
            Action a = null;
            a(); // FN

            Func<object> f = null;
            f(); // FN

            Action notNull = () => { };
            (condition ? a : notNull)();  // FN
        }

        void Assert1()
        {
            System.Diagnostics.Debug.Assert(_foo1 != null);
            _foo1.ToString(); // Compliant
            System.Diagnostics.Debug.Assert(_foo1 == null);
            _foo1.ToString(); // Compliant, because we already know that _foo1 is not null from _foo1.ToString()
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

        void CallToMethodsShouldResetFieldConstraintsWithTernaryInArgument(bool condition)
        {
            _foo1 = null;
            this.DoSomething(condition ? 1 : 2);
            _foo1.ToString(); // Compliant
        }

        void CallToExtensionMethodsShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            this.MyExtension();
            _foo1.ToString(); // Compliant
        }

        void CallToStaticMethodsShouldNotResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            Console.WriteLine(); // This particular method has no side effects
            _foo1.ToString(); // Noncompliant
            o.ToString(); // Noncompliant, local variable constraints are not cleared
        }

        static void CallToStaticMethodsShouldResetFieldConstraintsOfContainingClass()
        {
            object o = null;
            NullPointerDereferenceWithFields._foo8 = o;
            NullPointerDereferenceWithFields.CallToStaticMethodsShouldResetFieldConstraintsOfContainingClass();
            NullPointerDereferenceWithFields._foo8.ToString(); // Compliant. State is reset after calls of the same containg class
            o.ToString(); // Noncompliant, local variable constraints are not cleared
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/947
        void CallToMonitorWaitShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            System.Threading.Monitor.Wait(this); // This is a multi-threaded application, the fields could change
            _foo1.ToString(); // Noncompliant FP (was compliant before)
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

        void DoSomething(object o = null) { }

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

        void TryCatch4(object arg)
        {
            object a = null;
            try
            {
                ToString();           // Visit catch the first time
                var b = new object(); // Introduce new "state" to force the second visit of catch
                ToString();           // Visit catch the second time
            }
            catch
            {
                a.ToString(); // Noncompliant - only one issue should be reported
            }
        }

        void TryCatch5()
        {
            try
            {
                bool.Parse("No");
            }
            catch (Exception ex)
            {
                if (ex == null)
                {
                    ex.ToString(); // Unreachable. Any caught exception is never null
                }
            }
        }
    }

    static class Conversions
    {
        static void As(object o)
        {
            (o as string).ToString();
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
                this.bar.GetHashCode(); // Compliant
        }
    }

    public static class GuardedTests
    {
        public static void Guarded(string s1)
        {
            Guard1(s1);

            if (s1 == null)
                s1.ToUpper(); // Compliant, this code is unreachable
        }

        public static void Guard1<T>([ValidatedNotNull] T value) where T : class { }

        // https://github.com/SonarSource/sonar-dotnet/issues/3850
        public static void UsedAsExtension()
        {
            object a = null;
            if (a.IsNotNull())
            {
                a.ToString(); // Compliant
            }
        }

        public static void UsedAsMethod()
        {
            object a = null;
            if (IsNotNull(a))
            {
                a.ToString(); // Compliant
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
            return s.Trim(); // Compliant
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
            foreach (var item in list)
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
            return item.ToString(); // Noncompliant
        }

        string DoSomethingArg(IEnumerable<object> list)
        {
            var item = list.SingleOrDefault(x => x != null);
            return item.ToString(); // Noncompliant
        }

        string ValueTyped(IEnumerable<int> list)
        {
            var item = list.SingleOrDefault(x => x != 0);
            return item.ToString(); // Compliant, cannot be null
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/4537
    class Repro_4537
    {
        private void ConditionalAccess_NullCoalescing()
        {
            string someString = null;

            if (!someString?.Contains("a") ?? true)
                someString.ToString();  // Noncompliant, it's null or doesn't contain 'a'
            else
                someString.ToString();  // Compliant, someString is not null and contains 'a'
        }
    }

    public class Conditional
    {
        public void StringIsNullOrEmpty_Invocation(object arg)
        {
            if (string.IsNullOrEmpty(arg?.ToString()))
            {
                arg.ToString(); // Noncompliant
            }
            else
            {
                arg.ToString(); // Compliant
            }
        }

        public void StringIsNullOrEmpty_Property(Exception arg)
        {
            if (string.IsNullOrEmpty(arg?.Message))
            {
                arg.ToString(); // Noncompliant
            }
            else
            {
                arg.ToString(); // Compliant
            }
        }

        public void StringIsNullOrEmpty_Property2(string arg)
        {
            if (arg?.Length == 0)
            {
                arg.ToString(); // Compliant
            }
            else
            {
                arg.ToString(); // Noncompliant
            }
        }
    }

    public class Nancy_Repro
    {
        public void WithDynamic(string first, dynamic second)
        {
            if (first == null && second == null)
            {
                throw new Exception("Both first and second are null");
            }
            if (second == null && first.Length == 0)     // Noncompliant FP, if both first and second were null, an exception was already thrown
            {
                // Do something
            }
        }

        public void WithObject(string first, object second)
        {
            if (first == null && second == null)
            {
                throw new Exception("Both first and second are null");
            }
            if (second == null && first.Length == 0)     // Compliant
            {
                // Do something
            }
        }
    }

    public class Akka_Repro
    {
        public void NestedForEachTryForEach(IEnumerable<Exception> exceptions)
        {
            foreach (var ex in exceptions)
            {
                try
                {
                    foreach (var ch in ex.Message)  // Compliant, string.GetEnumerator returns not null
                    {
                        // Do something
                    }
                }
                catch
                {
                    // Do something
                }
            }
        }

        public void IsType(object message)
        {
            var first = message as IFirst;
            var second = message as ISecond;
            if (first != null)
            {
                // Do something
            }
            else if (second?.Property is string)
            {
                var str = second.Property; // Compliant
            }
        }

        private interface IFirst { }
        private interface ISecond { object Property { get; } }
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
                o.ToString(); // Noncompliant (old engine: FN, the condition state generation is too big to explore all constraint combinations)
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
                o.ToString(); // Noncompliant (old engine: FN, the condition state generation is too big to explore all constraint combinations)
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
                o.ToString(); // Compliant, helper is A so helper==B produces false, and the entire expression si false
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
                o.ToString(); // Noncompliant (old engine: FN, the condition state generation is too big to explore all constraint combinations)
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
                o.ToString(); // Noncompliant (old engine: FN, the condition state generation is too big to explore all constraint combinations)
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
                o.ToString(); // Compliant, unreachable
            }
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

        return test.Count; // Compliant
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
                continue;
            }

            _ = providerCourse.Items; // Compliant
        }

        foreach (var providerCourse in providerCourses)
        {
            if (!providerCourse?.Items?.Any() ?? true)
            {
                Console.WriteLine("FAIL");
                continue;
            }

            _ = providerCourse.Items.Where(items => items == "item1"); // Compliant
        }
    }

    class ProviderCourse
    {
        public IEnumerable<string> Items { get; set; }
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
            B(a.Length);
        }
    }
}

public class Repro_GridChecks
{
    public int Go(string first, string second)
    {
        if (first == second)
            return 0;
        if (first != null && second == null)
            return -1;
        if (first == null && second != null)
            return 1;

        first.Split('.');   // Noncompliant FP
        second.Split('.');  // Noncompliant FP

        return 0;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/890
public class Repo_890
{
    public void M()
    {
        Exception lastEx = null;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                ToString(); // May throw
                return;
            }
            catch (InvalidOperationException e)
            {
                lastEx = e;
            }
        }
        lastEx.ToString(); // Compliant. The loop is always entered and so lastEx is never null here.
    }
}

namespace ValidatedNotNullAttributeTest
{
    public sealed class ValidatedNotNullAttribute : Attribute { }
    public sealed class NotNullAttribute : Attribute { }

    public static class Guard
    {
        public static void ValidatedNotNull<T>([ValidatedNotNullAttribute] this T value, string name) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        public static void NotNull([NotNull] object value) { }
    }

    public static class Utils
    {
        public static string ToUpper(string value)
        {
            Guard.ValidatedNotNull(value, nameof(value));
            if (value != null)
            {
                return value.ToUpper(); // Compliant Unreachable
            }
            return value.ToUpper(); // Compliant
        }

        public static string NotNullTest(string value)
        {
            Guard.NotNull(value);
            if (value != null)
            {
                return value.ToUpper(); // Compliant Unreachable
            }
            return value.ToUpper(); // Compliant
        }
    }
}

namespace DoesNotReturnIf
{
    public sealed class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue) { }
    }

    public class Sample
    {
        private object notImportant;

        public void ForTrue(object arg)
        {
            var canBeNull = arg?.ToString();
            FailsWhenTrue(notImportant, canBeNull == null, notImportant);
            canBeNull.ToString();    // Compliant
        }

        public void ForTrueFromUnknown(object arg)
        {
            FailsWhenTrue(notImportant, arg == null, notImportant);
            if (arg == null)
            {
                arg.ToString();    // Compliant, unreachable
            }
        }

        public void ForTrueAny(object arg1, object arg2, bool condition)
        {
            var canBeNull1 = arg1 == null ? null : arg1.ToString();
            var canBeNull2 = arg2 == null ? null : arg2.ToString();
            FailsWhenTrueAny(canBeNull1 == null, condition);
            canBeNull1.ToString();  // Compliant
            FailsWhenTrueAny(condition, canBeNull2 == null);
            canBeNull2.ToString();  // Compliant
            if (arg1 == null || arg2 == null)
            {
                arg1.ToString();    // Compliant, unreachable
                arg2.ToString();
            }
        }

        public void ForTrueAny(object arg1, object arg2)
        {
            var canBeNull1 = arg1 == null ? null : arg1.ToString();
            var canBeNull2 = arg2 == null ? null : arg2.ToString();
            FailsWhenTrueAny(canBeNull1 == null, canBeNull2 == null);
            canBeNull1.ToString();  // Compliant
            canBeNull2.ToString();  // Compliant
            if (arg1 == null || arg2 == null)
            {
                arg1.ToString();    // Compliant, unreachable
                arg2.ToString();
            }
        }

        public void ForFalse(object arg)
        {
            var canBeNull = arg?.ToString();
            FailsWhenFalse(notImportant, canBeNull != null, notImportant);
            canBeNull.ToString();    // Compliant
        }

        public void ForFalseAny(object arg1, object arg2, bool condition)
        {
            var canBeNull1 = arg1 == null ? null : arg1.ToString();
            var canBeNull2 = arg2 == null ? null : arg2.ToString();
            FailsWhenFalseAny(canBeNull1 != null, condition);
            canBeNull1.ToString();  // Compliant
            FailsWhenFalseAny(condition, canBeNull2 != null);
            canBeNull2.ToString();  // Compliant
            if (arg1 == null || arg2 == null)
            {
                arg1.ToString();    // Compliant, unreachable
                arg2.ToString();
            }
        }

        public void ForFalseAny(object arg1, object arg2)
        {
            var canBeNull1 = arg1 == null ? null : arg1.ToString();
            var canBeNull2 = arg2 == null ? null : arg2.ToString();
            FailsWhenFalseAny(canBeNull1 != null, canBeNull2 != null);
            canBeNull1.ToString();  // Compliant
            canBeNull2.ToString();  // Compliant
            if (arg1 == null || arg2 == null)
            {
                arg1.ToString();    // Compliant, unreachable
                arg2.ToString();
            }
        }

        public void BoolSymbols_TrueTrue()
        {
            var isTrue = true;
            object isNull = null;
            FailsWhenTrue(notImportant, isTrue, notImportant);
            isNull.ToString();  // Compliant, unreachable
        }

        public void BoolSymbols_TrueFalse()
        {
            var isTrue = true;
            object isNull = null;
            FailsWhenFalse(notImportant, isTrue, notImportant);
            isNull.ToString();  // Noncompliant
        }

        public void BoolSymbols_FalseTrue()
        {
            var isFalse = false;
            object isNull = null;
            FailsWhenTrue(notImportant, isFalse, notImportant);
            isNull.ToString();  // Noncompliant
        }

        public void BoolSymbols_FalseFalse()
        {
            var isFalse = false;
            object isNull = null;
            FailsWhenFalse(notImportant, isFalse, notImportant);
            isNull.ToString();  // Compliant, unreachable
        }

        public void FailsWhenTrue([CLSCompliant(false)] object before, [DoesNotReturnIf(true), CLSCompliant(false)] bool condition, object after) { }
        public void FailsWhenFalse(object before, [DoesNotReturnIf(false)] bool condition, object after) { }
        public void FailsWhenTrueAny([DoesNotReturnIf(true)] bool condition1, [DoesNotReturnIf(true)] bool condition2) { }
        public void FailsWhenFalseAny([DoesNotReturnIf(false)] bool condition1, [DoesNotReturnIf(false)] bool condition2) { }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6176
public class Repro_6176
{
    public void ExpressionArguments(object arg)
    {
        WithExpression(x => x.FirstOrDefault().ToString());         // Compliant - custom expression processor, like Entity Framework, can propagate the "null" without executing the actual code
        WithExpression(() => arg == null ? arg.ToString() : null);  // Compliant, we don't analyze arguments of expression

        System.Linq.Expressions.Expression<Func<IEnumerable<object>, object>> expr;
        expr = x => x.FirstOrDefault().ToString();          // Compliant as well
    }

    public void DelegateArguments(object arg)
    {
        WithDelegate(x => x.FirstOrDefault().ToString());         // Noncompliant
        WithDelegate(() => arg == null ? arg.ToString() : null);  // Noncompliant
    }

    public void Nested(object arg)
    {
        WithExpression(() => WithDelegate(x => x.FirstOrDefault().ToString()));   // Compliant, it's invoked lambda inside an expression tree
    }

    private void WithExpression(System.Linq.Expressions.Expression<Func<IEnumerable<object>, object>> expression) { }
    private void WithExpression(System.Linq.Expressions.Expression<Func<object>> expression) { }
    private object WithDelegate(Func<IEnumerable<object>, object> expression) => null;
    private void WithDelegate(Func<object> expression) { }
}

public class Peach_Sharex_Project
{
    public void TwoVariables(object[] argArray)
    {
        object item;
        if (argArray != null && argArray.Length > 1)
        {
            item = argArray[0];
        }
        else
        {
            item = null;
        }
        if (item != null)
        {
            argArray.ToString();    // Compliant
        }
    }

    public void StringIsNullOrEmpty(string value)
    {
        if (value == null)
        {
            value = "";
        }
        var isEmpty = string.IsNullOrEmpty(value);
        if (isEmpty)
        {
            value.ToString();   // Compliant
        }
        else
        {
            value.ToString();   // Compliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6241
public class Repro_6241<T>
{
    public void WithGenericValue()
    {
        HashSet<T> value = null;
        if (value is null)
        {
            value.ToString();   // Noncompliant
        }
        else
        {
            value.ToString();   // Compliant
        }
    }

    public void WithNormalValue()
    {
        HashSet<object> value = null;
        if (value is null)
        {
            value.ToString();   // Noncompliant
        }
        else
        {
            value.ToString();   // Compliant
        }
    }
}

public class PeachReproducers
{
    public void ConditionalPropertyInt(string value)
    {
        if (value?.Length > 0)
        {
            value.ToString();   // Compliant, positive, hence non-null
        }
    }

    public void ConditionalPropertyBool(Task t)
    {
        if (t?.IsCompleted == true)
        {
            t.ToString();       // Compliant
        }
    }

    public void ConditionalPropertyInvocation(bool condition)
    {
        var list = condition ? null : new List<string>();
        if (list?.Contains("key") == true)
        {
            list.ToString();    // Compliant
        }
    }

    public void ConditionalIntTrackingZero(string[] arr)
    {
        if ((arr?.Length ?? 0) == 0)
        {
            return;
        }
        arr.ToString();         // Compliant
    }

    public void ConditionalIntTrackingMinusOne(string value)
    {
        var index = value?.IndexOf("x") ?? -1;
        if (index != -1)
        {
            value.ToString();   // Compliant
        }
    }

    public void ConditionalIntTrackingOne(string value)
    {
        var index = value != null ? value.Length : 0;
        if (index == 1)
        {
            value.ToString();   // Compliant
        }
    }

    public void ConditionalIntTrackingOne_InsideLoop(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var value = Unknown();
            var len = value == null ? 0 : value.Length;
            if (len == 1)
            {
                value.ToString();   // Compliant
            }
        }
    }

    private string Unknown() => null;

    public void WithDynamic(IEnumerable<Exception> list, dynamic message, dynamic displayName)
    {
        {
            var ex = list.FirstOrDefault();
            if (ex != null && !string.IsNullOrEmpty(message) && ex.Message != message) // Noncompliant FP
            {
                ex.ToString();  // Noncompliant FP
            }
        }
    }

    public void WhileTryForEachQuery(IList<Exception> list)
    {
        var run = true;
        while (run)
        {
            try
            {
                foreach (var msg in from ex in list select ex.Message)              // Noncompliant {{'from ex in list select ex.Message' is null on at least one execution path.}} FP
                {
                }
                foreach (var msg in (from ex in list select ex.Message).Distinct()) // Noncompliant {{'(from ex in list select ex.Message).Distinct()' is null on at least one execution path.}} FP
                {
                }
                var subList = SubList();
                foreach (var subItem in subList)    // Noncompliant FP
                {
                }
                run = false;
            }
            catch
            {
            }
        }

        IEnumerable<string> SubList() =>
            Array.Empty<string>();
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7104
public class Repro_7104
{
    private Data data;

    public static bool BoolPropertyPatternMatching(Data data)
    {
        if (data?.IsBoolean is true && data.Property != null)    // Compliant
        {
            return true;
        }
        return false;
    }

    public static bool BoolMethodPatternMatching(Data data)
    {
        if (data?.IsTrue() is true && data.Property != null)    // Compliant
        {
            return true;
        }
        return false;
    }

    private object BoolProperty()
    {
        if (data?.IsBoolean == true)
        {
            return data.Property;   // Compliant
        }
        return null;
    }

    public static int BoolExtensionNegated<T>(ICollection<T> records)
    {
        if (records?.Any() != true)
            return 0;

        return records.Count; // Compliant
    }

    public class Data
    {
        public bool IsBoolean { get; set; }
        public object Property { get; set; }

        public bool IsTrue() => true;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8266
public class Repro_8266
{
    void Method(Exception[] array)
    {
        if (array.Any())
        {
            Exception exception = array.FirstOrDefault();
            Console.WriteLine(exception.Message); // Compliant - we know that array is not empty
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8324
class Repro_8324
{
    void Foo(object o, int i)
    {
        object n = null;

        if (i is var x1)
            Console.WriteLine();
        else
            Console.WriteLine(n.ToString());    // Compliant: unreachable

        if (o is var x2)
            Console.WriteLine();
        else
            Console.WriteLine(n.ToString());    // Compliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8905
class Repro_8905
{
    public string Value { get; private set; }
    private void Foo()
    {
        string value = null;
        if (Value != null)                      // Branching on Value
        {
            value = "";
        }

        if (Value != null)                      // Both branches enter here because PropertyReference is not learning from TrackedSymbol
        {
            Console.WriteLine(value.Length);    // Compliant
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8907
// https://github.com/SonarSource/sonar-dotnet/issues/8908
class Repro_8907_8908
{
    private void Foo(string[] values)
    {
        if (values == null)
        {
            Console.WriteLine("Oops!");
        }
        if (5 < values.Count())         // FN, will throw when values is null
        {
            _ = values[5].ToString();   // Compliant, we learn NotNull from LINQ methods
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8910
class Repro_8910
{
    private void Foo(List<string> values)
    {
        string x = null;
        if (values.Count > 0)
        {
            x = "";
        }
        foreach (var value in values)
        {
            _ = x.Length;   // Noncompliant FP
        }
    }
}

public class Repro_NoCollectionInstanceOnFirstOrDefault
{
    public IQueryable<AClass> Elements { get; set; }
    public IQueryable<AStruct> StructElements { get; set; }

    public void Put(long id, AClass model)
    {
        var element = Elements.Where(x => x.Name == "name").FirstOrDefault(x => x.Id == id);
        _ = element.Name;      // Noncompliant

        var structElement = StructElements.Where(x => x.Name == "name").FirstOrDefault(x => x.Id == id);
        _ = structElement.Name; // Compliant, structs are never null
    }

    public class AClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public struct AStruct
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
