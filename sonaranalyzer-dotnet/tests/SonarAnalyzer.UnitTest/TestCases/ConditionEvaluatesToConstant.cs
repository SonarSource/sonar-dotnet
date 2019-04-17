using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Tests.Diagnostics
{
    public class ConditionEvaluatesToConstant
    {
        private const bool t = true;
        private const bool f = false;

        public void LoopsWithBreak(object o1, object o2, object o3)
        {
            bool c1, c2, c3;
            c1 = c2 = c3 = true;

            while (c1)
            {
                if (o1 != null)
                    break;
            }

            do
            {
                if (o2 != null)
                    break;
            } while (c2);

            for (int i = 0; c3; i++)
            {
                if (o3 != null)
                    break;
            }
        }

        public void NotExecutedLoops(object o1, object o2, object o3)
        {
            bool c1, c2, c3;
            c1 = c2 = c3 = false;

            while (c1) // Noncompliant
            { // Secondary
                if (o1 != null)
                    break;
            }

            do
            { // Secondary
                if (o2 != null)
                    break;
            } while (c2); // Noncompliant

            for (int i = 0; c3; i++) // Noncompliant
            {
                if (o3 != null)
                    break;
            }
        }

        public void BreaksInLoop(object o1, object o2, object o3)
        {
            bool c1, c2, c3;
            c1 = c2 = c3 = true;

            while (c1)
            {
                if (o1 != null)
                    break;
            }

            while (c2)
            {
                if (o2 != null)
                    return;
            }

            while (c3)
            {
                if (o3 != null)
                    throw new Exception();
            }
        }

        public void Foo1(bool a, bool b)
        {
            var x = t || a || b;
//                  ^ {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                       ^^^^^^ Secondary@-1
        }

        public void Foo2(bool a, bool b)
        {
            var x = ((t)) || a || b;
//                    ^ {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                           ^^^^^^ Secondary@-1

        }

        public void Foo3(bool a, bool b)
        {
            var x = ((t || a)) || b;
//                    ^ {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                         ^^^^^^^^ Secondary@-1

        }

        public void Foo4(bool a, bool b)
        {
            var x = ((f || t)) || a || b;
//                         ^ Noncompliant {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                    ^ Noncompliant@-1 {{Change this condition so that it does not always evaluate to 'false'.}}
//                                ^^^^^^ Secondary@-2

        }

        public void Foo5(bool a, bool b)
        {
            var x = ((f && t)) || a || b;
//                    ^ Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
//                         ^ Secondary@-1
        }

        public void Foo6(bool a, bool b)
        {
            var x = t || a ? a : b;
//                  ^ Noncompliant {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                       ^ Secondary@-1
//                               ^ Secondary@-2
        }

        public void Foo7(bool a, bool b)
        {
            if ((t || a ? a : b) || b)
//               ^ Noncompliant {{Change this condition so that it does not always evaluate to 'true'; some subsequent code is never executed.}}
//                    ^ Secondary@-1
//                            ^ Secondary@-2
            {
            }
        }

        void Pointer(int* a) // Error [CS0214]
        {
            if (a != null)  // Error [CS0214]
                            // Compliant
            {
            }
        }

        void Nameof(string s)
        {
            if (null == nameof(Method1)) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }
        }

        public void Method1()
        {
            var b = true;
            if (b) // Noncompliant
//              ^
            {
                Console.WriteLine();
            }
            else
            { // Secondary
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method2()
        {
            var b = true;
            if (b) // Noncompliant
            {
                Console.WriteLine();
            }

            if (!b) // Noncompliant
            { // Secondary
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method2Literals()
        {
            if (true) // Compliant
            {
                Console.WriteLine();
            }

            if (false) // Compliant
            {
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method3()
        {
            bool b;
            TryGet(out b);
            if (b) { }
        }

        private void TryGet(out bool b) { b = false; }

        public void Method5(bool cond)
        {
            while (cond)
            {
                Console.WriteLine();
            }

            var b = true;
            while (b) // Noncompliant
            {
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method6(bool cond)
        {
            var i = 10;
            while (i < 20)
            {
                i = i + 1;
            }

            var b = true;
            while (b) // Noncompliant
            {
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method7(bool cond)
        {
            while (true) // Compliant, this is too common to report on
            {
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public void Method8(bool cond)
        {
            foreach (var item in new int[][] { new int[] { 1, 2, 3 } })
            {
                foreach (var i in item)
                {
                    Console.WriteLine();
                }
            }
        }

        public void Method9_For(bool cond)
        {
            for (;;) // Not reporting on this
            {

            }
        }

        public void Method_Switch()
        {
            int i = 10;
            bool b = true;
            switch (i)
            {
                case 1:
                default:
                case 2:
                    b = false;
                    break;
                case 3:
                    b = false;
                    break;
            }

            if (b) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }
            else
            { }
        }

        public void Method_Switch_NoDefault()
        {
            int i = 10;
            bool b = true;
            switch (i)
            {
                case 1:
                case 2:
                    b = false;
                    break;
            }

            if (b)
            {
            }
            else
            {
            }
        }

        public void Method_Switch_Learn(bool cond)
        {
            switch (cond)
            {
                case true:
                    if (cond) // Non-compliant, we don't care it's very rare
                    {
                        Console.WriteLine();
                    }
                    break;
            }
        }

        public bool Property1
        {
            get
            {
                var a = new Action(() =>
                {
                    var b = true;
                    if (b) // Noncompliant
                    {
                        Console.WriteLine();
                    }
                    else
                    { // Secondary
                        Console.WriteLine();
                    }
                });
                return true;
            }
            set
            {
                value = true;
                if (value) // Noncompliant
//                  ^^^^^
                {
                    Console.WriteLine();
                }
                else
                { // Secondary
                    Console.WriteLine();
                }
            }
        }

        public static bool T
        {
            get
            {
                return t;
            }
        }

        public void Method_Complex()
        {
            bool guard1 = true;
            bool guard2 = true;
            bool guard3 = true;

            while (GetCondition())
            {
                if (guard1)
                {
                    guard1 = false;
                }
                else
                {
                    if (guard2) // Noncompliant, false-positive
                    {
                        guard2 = false;
                    }
                    else
                    { // Secondary
                        guard3 = false;
                    }
                }
            }

            if (guard3) // Noncompliant, false-positive, kept only to show that problems with loops can cause issues outside the loop
            {
                Console.WriteLine();
            }
        }

        public void Method_Complex_2()
        {
            var x = false;
            var y = false;

            while (GetCondition())
            {
                while (GetCondition())
                {
                    if (x)
                    {
                        if (y) // Noncompliant, false-positive
                        { // Secondary
                        }
                    }
                    y = true;
                }
                x = true;
            }
        }
        static object GetObject() { return null; }
        public void M()
        {
            var o1 = GetObject();
            object o2 = null;
            if (o1 != null)
            {
                if (o1.ToString() != null)
                {
                    o2 = new object();
                }
            }
            if (o2 == null)
            {

            }
        }

        public void NullableStructs()
        {
            int? i = null;

            if (i == null) // Noncompliant, always true
            {
                Console.WriteLine(i);
            }

            i = new Nullable<int>();
            if (i == null) // Noncompliant
            { }

            int ii = 4;
            if (ii == null) // Noncompliant, always false
            { // Secondary
                Console.WriteLine(ii);
            }
        }

        private static bool GetCondition()
        {
            return true;
        }

        public void Lambda()
        {
            var fail = false;
            Action a = new Action(() => { fail = true; });
            a();
            if (fail) // This is compliant, we don't know anything about 'fail'
            {
            }
        }

        public void Constraint(bool cond)
        {
            var a = cond;
            var b = a;
            if (a)
            {
                if (b) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                {

                }
            }
        }

        public void Stack(bool cond)
        {
            var a = cond;
            var b = a;
            if (!a)
            {
                if (b) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                { // Secondary
                }
            }

            var fail = false;
            Action ac = new Action(() => { fail = true; });
            ac();
            if (!fail) // This is compliant, we don't know anything about 'fail'
            {
            }
        }

        public void BooleanBinary(bool a, bool b)
        {
            if (a & !b)
            {
                if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }

            if (!(a | b))
            {
                if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }

            if (a ^ b)
            {
                if (!a ^ !b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            a = false;
            if (a & b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            // Secondary@-1

            a &= true;
            if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            // Secondary@-1

            a |= true;
            if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}

            a ^= true;
            if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            // Secondary@-1

            a ^= true;
            if (a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
        }

        public void IsAsExpression()
        {
            object o = new object();
            if (o is object) { }
            var oo = o as object;
            if (oo == null) { }

            o = null;
            if (o is object) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            // Secondary@-1
            oo = o as object;
            if (oo == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
        }

        public void Equals(bool b)
        {
            var a = true;
            if (a == b)
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }
            else
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }

            if (!(a == b))
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
            else
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }
        }

        public void NotEquals(bool b)
        {
            var a = true;
            if (a != b)
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
            else
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (!(a != b))
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }
            else
            {
                if (b) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
        }

        public void EqRelations(bool a, bool b)
        {
            if (a == b)
            {
                if (b == a) { }    // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (b == !a) { }   // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
                if (!b == !!a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
                if (!(a == b)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
            else
            {
                if (b != a) { }    // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (b != !a) { }   // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
                if (!b != !!a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }

            if (a != b)
            {
                if (b == a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
            else
            {
                if (b != a) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                // Secondary@-1
            }
        }

        public void RelationshipWithConstraint(bool a, bool b)
        {
            if (a == b && a) { if (b) { } } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
//                                 ^
            if (a != b && a)
            {
                if (b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                           // Secondary@-1
            }

            if (a && b)
            {
                if (a == b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (a && b && a == b) { } // Noncompliant
//                        ^^^^^^

            a = true;
            b = false;
            if (a &&        // Noncompliant
                b)          // Noncompliant
            { // Secondary
            }
        }

        private static void BackPropagation(object a, object b)
        {
            if (a == b && b == null)
            {
                a.ToString();
            }
        }

        public void RefEqualsImpliesValueEquals(object a, object b)
        {
            if (object.ReferenceEquals(a, b))
            {
                if (object.Equals(a, b)) { }    // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (Equals(a, b)) { }           // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (a.Equals(b)) { }            // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (this == a)
            {
                if (this.Equals(a)) { } // Noncompliant
                if (Equals(a)) { }      // Noncompliant
            }
        }

        public void ValueEqualsDoesNotImplyRefEquals(object a, object b)
        {
            if (object.Equals(a, b)) // 'a' could override Equals, so this is not a ref equality check
            {
                if (a == b) { } // Compliant
            }
        }

        public void ReferenceEqualsMethodCalls(object a, object b)
        {
            if (object.ReferenceEquals(a, b))
            {
                if (a == b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (a == b)
            {
                if (object.ReferenceEquals(a, b)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }
        }

        public void ReferenceEqualsMethodCallWithOpOverload(ConditionEvaluatesToConstant a, ConditionEvaluatesToConstant b)
        {
            if (object.ReferenceEquals(a, b))
            {
                if (a == b) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (a == b)
            {
                if (object.ReferenceEquals(a, b)) { } // Compliant, == is doing a value comparison above.
            }
        }

        public void ReferenceEquals(object a, object b)
        {
            if (object.ReferenceEquals(a, b)) { }

            if (object.ReferenceEquals(a, a)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}

            a = null;
            if (object.ReferenceEquals(null, a)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}

            if (object.ReferenceEquals(null, new object())) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            int i = 10;
            if (object.ReferenceEquals(i, i)) { } // Noncompliant because of boxing {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            int? ii = null;
            int? jj = null;
            if (object.ReferenceEquals(ii, jj)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}

            jj = 10;
            if (object.ReferenceEquals(ii, jj)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1
        }

        public void ReferenceEqualsBool(bool a, bool b)
        {
            if (object.ReferenceEquals(a, b)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1
        }

        public void ReferenceEqualsNullable(int? ii, int? jj)
        {
            if (object.ReferenceEquals(ii, jj)) { } // Compliant, they might be both null
            jj = 1;
            if (object.ReferenceEquals(ii, jj)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(ConditionEvaluatesToConstant a, ConditionEvaluatesToConstant b)
        {
            return false;
        }

        public static bool operator !=(ConditionEvaluatesToConstant a, ConditionEvaluatesToConstant b)
        {
            return false;
        }

        public void StringEmpty(string s1)
        {
            string s = null;
            if (string.IsNullOrEmpty(s)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            if (string.IsNullOrWhiteSpace(s)) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            if (string.IsInterned(s) != null) { }
            s = "";
            if (string.IsNullOrEmpty(s)) { } // Noncompliant
//Secondary@-1
            if (string.IsNullOrWhiteSpace(s)) { } // Noncompliant
//Secondary@-1

            if (string.IsNullOrEmpty(s1)) { } // Compliant, we don't know anything about the argument
            if (string.IsNullOrWhiteSpace(s1)) { } // Compliant

            if (string.IsNullOrEmpty(s1))
            {
                if (string.IsNullOrEmpty(s1)) { } // Noncompliant, we know s1 is not null -> this is always false
// Secndary@-1
            }
        }

        public void Comparisons(int i, int j)
        {
            if (i < j)
            {
                if (j < i) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                //Secondary@-1
                if (j <= i) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                //Secondary@-1
                if (j == i) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                //Secondary@-1
                if (j != i) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }

            if (i <= j)
            {
                if (j < i) { }  // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                //Secondary@-1
                if (j <= i)
                {
                    if (j == i) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                    if (j != i) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                    //Secondary@-1
                }
                if (j == i)
                {
                    if (i >= j) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                }
                if (j != i)
                {
                    if (i >= j) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                                    //Secondary@-1
                }
            }
        }

        void ValueEquals(int i, int j)
        {
            if (i == j)
            {
                if (Equals(i, j)) { } // Noncompliant
                if (i.Equals(j)) { }  // Noncompliant
            }
        }

        void DefaultExpression(object o)
        {
            if (default(object) == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            int? nullableInt = null;
            if (nullableInt == null) { } // Noncompliant
            if (default(int?) == null) { } // Noncompliant

            if (default(System.IO.FileAccess) != null) { } // Noncompliant
            if (default(float) != null) { } // Noncompliant
        }

        void DefaultGenericClassExpression<TClass>(TClass arg)
            where TClass : class
        {
            if (default(TClass) == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
        }

        void DefaultGenericStructExpression<TStruct>(TStruct arg)
            where TStruct : struct
        {
            if (default(TStruct) != null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                                              // Error@-1 [CS0019]
        }

        void DefaultUnconstrainedGenericExpression<T>(T arg)
        {
            if (default(T) == null) { } // We know nothing about T
        }

        void ConditionalAccessNullPropagation(object o)
        {
            if (o == null)
            {
                if (o?.ToString() == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                if (o?.GetHashCode() == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
            }
        }

        void Cast()
        {
            var i = 5;
            var o = (object)i;
            if (o == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            var x = (ConditionEvaluatesToConstant)o; // This would throw and invalid cast exception
            if (x == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1
        }

        public async Task NotNullAfterAccess(object o, int[,] arr, IEnumerable<int> coll, Task task)
        {
            Console.WriteLine(o.ToString());
            if (o == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            Console.WriteLine(arr[42, 42]);
            if (arr == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            foreach (var item in coll)
            {
            }
            if (coll == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1

            await task;
            if (task == null) { } // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            //Secondary@-1
        }

        public void EnumMemberAccess()
        {
            var m = new MyClass();
            Console.WriteLine(m.myEnum);
            m = null;
            if (m?.myEnum == MyEnum.One) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }
        }

        int field;
        int GetValue() { return 42; }
        public void NullabiltyTest()
        {
            if (field == null)  // Noncompliant
            { // Secondary
            }

            int i = GetValue();
            if (i == null)      // Noncompliant
            { // Secondary
            }
        }

        public void EqualsTest32(object o)
        {
            var o2 = o;
            if (o == o2) { } // Noncompliant
            if (object.ReferenceEquals(o, o2)) { } // Noncompliant
            if (o.Equals(o2)) { } // Noncompliant
            if (object.Equals(o2, o)) { } // Noncompliant


            int i = 1;
            int j = i;
            if (i == j) // Noncompliant
            {
            }

            if (i.Equals(j)) { } // Noncompliant
            if (object.Equals(i, j)) { } // Noncompliant
        }

        async Task Test_Await_Constraint(Task<string> t)
        {
            if (t != null)
            {
                var o = await t;
                if (o == null) { } // Compliant, might be null
            }
        }

        enum MyEnum
        {
            One, Two
        }

        class MyClass
        {
            public MyEnum myEnum;
        }

        public void Assert(bool condition, object o1)
        {
            Debug.Assert(condition);

            if (condition) // Compliant
            {
            }

            Trace.Assert(condition); // Compliant

            if (o1 != null)
            {
                Debug.Assert(o1 == null, "Some message", "More details", 1, 2, 3); // Compliant
            }
        }

        public void Assert(object o1)
        {
            System.Diagnostics.Debug.Assert(o1 != null);
            System.Diagnostics.Debug.Assert(o1 == null);
        }

        void ComparisonTransitivity(int a, int b, int c)
        {
            if (a == b && b < c)
            {
                if (a >= c) { } // Noncompliant
                                //Secondary@-1
            }
            if (a == b && b <= c)
            {
                if (a > c) { } // Noncompliant
                               //Secondary@-1
            }
            if (a > b && b > c)
            {
                if (a <= c) { } // Noncompliant
                                //Secondary@-1
            }
            if (a > b && b >= c)
            {
                if (a <= c) { } // Noncompliant
                                //Secondary@-1
            }
            if (a >= b && b >= c)
            {
                if (a < c) { } // Noncompliant
                               //Secondary@-1
            }
            if (a >= b && c <= b)
            {
                if (a < c) { } // Noncompliant
                               //Secondary@-1
            }
            if (a >= b && c >= b)
            {
                if (a < c) { }
            }
        }

        void RefEqTransitivity(Comp a, Comp b, Comp c)
        {
            if (a == b && b == c)
            {
                if (a != c) { } // Noncompliant
                                //Secondary@-1
            }
            if (a.Equals(b) && b == c)
            {
                if (a != c) { }
                if (a == c) { }
                if (a.Equals(c)) { }  // Noncompliant
                if (!a.Equals(c)) { } // Noncompliant
                                      //Secondary@-1
            }
            if (a > b && b == c)
            {
                if (a <= c) { } // Noncompliant
                                //Secondary@-1
            }
        }

        void ValueEqTransitivity(Comp a, Comp b, Comp c)
        {
            if (a == b && b.Equals(c))
            {
                if (a.Equals(c)) { } // Noncompliant
            }
            if (a.Equals(b) && b.Equals(c))
            {
                if (a != c) { }
                if (a == c) { }
                if (a.Equals(c)) { }  // Noncompliant
                if (!a.Equals(c)) { } // Noncompliant
                                      //Secondary@-1
            }
            if (a > b && b.Equals(c))
            {
                if (a > c) { } // Noncompliant
                if (a <= c) { } // Noncompliant
                                //Secondary@-1
            }
            if (!a.Equals(b) && b.Equals(c))
            {
                if (a.Equals(c)) { } // Noncompliant
                                     //Secondary@-1
                if (a == c) { } // Noncompliant
                                //Secondary@-1
            }
            if (a != b && b.Equals(c))
            {
                if (a.Equals(c)) { }
                if (a == c) { }
            }
        }

        void NeqEqTransitivity(object a, object b, object c)
        {
            if (a == c && a != b)
            {
                if (b == c) { } // Noncompliant
                                //Secondary@-1
                if (b.Equals(c)) { }
            }

            if (a == c && !a.Equals(b))
            {
                if (b == c) { }         // Noncompliant
                                        //Secondary@-1
                if (b.Equals(c)) { }    // Noncompliant
                                        //Secondary@-1
            }
        }

        public void LiftedOperator()
        {
            int? i = null;
            int? j = 5;

            if (i < j) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i <= j) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i > j) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i >= j) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i > 0) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i >= 0) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i < 0) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (i <= 0) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (j > null) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (j >= null) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (j < null) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }

            if (j <= null) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
            { // Secondary
            }
        }

        unsafe void Pointers(int* a, int* b)
        {
            if (a < b)
            {
            }
        }

        class Singleton
        {
            private static object syncRoot = new object();

            private static Singleton instance;

            public static Singleton Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (syncRoot)
                        {
                            if (instance == null) // Compliant
                            {
                                instance = new Singleton();
                            }
                        }
                    }
                    return instance;
                }
            }
        }

        class Comp
        {
            public static bool operator <(Comp a, Comp b) { return true; }
            public static bool operator >(Comp a, Comp b) { return true; }
            public static bool operator >=(Comp a, Comp b) { return true; }
            public static bool operator <=(Comp a, Comp b) { return true; }
        }

        struct MyStructWithNoOperator
        {
            public static void M(MyStructWithNoOperator a)
            {
                if (a == null) // Noncompliant, also a compiler error
                               // Error@-1 [CS0019]
                { // Secondary
                }
            }
        }

        struct MyStructWithOperator
        {
            public static bool operator ==(MyStructWithOperator? a, MyStructWithOperator? b)
            {
                return true;
            }

            public static bool operator !=(MyStructWithOperator? a, MyStructWithOperator? b)
            {
                return true;
            }

            public static void M(MyStructWithOperator a)
            {
                if (a == null) // Compliant
                {
                }
            }
        }

        public class NullableCases
        {
            void Case1()
            {
                bool? b1 = true;
                if (b1 == true) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                {

                }
            }

            void Case2()
            {
                bool? b2 = true;
                if (b2 == false) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                { // Secondary
                }

                bool? b3 = true;
                if (b3 == null) // Should be NC
                {
                }

                bool? b4 = null;
                if (b4 == true) // Should be NC
                {
                }

                bool? b5 = null;
                if (b5 == false) // Should be NC
                {
                }
            }

            void Case3(bool? b)
            {
                if (b == null)
                {
                    if (null == b) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                    {
                        b.ToString();
                    }
                }
                else
                {
                    if (b != null) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                    {
                        b.ToString();
                    }
                }
            }

            void Case4(bool? b)
            {
                if (b == true)
                {
                    if (true == b) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                    {
                        b.ToString();
                    }
                }
            }

            void Case5(bool? b)
            {
                if (b == true)
                {
                }
                else if (b == false)
                {
                }
                else
                {
                }
            }

            void Case6(bool? b)
            {
                if (b == null)
                {
                }
                else if (b == true)
                {
                }
                else
                {
                }
            }

            void Case7(bool? b)
            {
                if (b == null)
                {
                    if (b ?? false) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                    { // Secondary

                    }
                }
            }

            void Case8(bool? b)
            {
                if (b != null)
                {
                    if (b.HasValue) // Noncompliant {{Change this condition so that it does not always evaluate to 'true'.}}
                    {
                    }
                }
            }

            void Case9(bool? b)
            {
                if (b == true)
                {
                    var x = b.Value;
                    if (x == true) // TODO: Should be NC {{Change this condition so that it does not always evaluate to 'true'.}}
                    {
                    }
                }
            }

            void Case10(int? i)
            {
                if (i == null)
                {
                    if (i.HasValue) // Noncompliant {{Change this condition so that it does not always evaluate to 'false'; some subsequent code is never executed.}}
                    { // Secondary
                    }
                }
            }
        }

        public class ConstantExpressionsAreExcluded
        {
            const bool T = true;
            const bool F = false;

            void LocalConstants()
            {
                const bool t = true;
                if (t)
                {
                    Console.WriteLine();
                }
                const bool f = false;
                if (f)
                {
                    Console.WriteLine();
                }
            }
            void Constants()
            {
                if (ConstantExpressionsAreExcluded.T)
                {
                    Console.WriteLine();
                }
                if (ConstantExpressionsAreExcluded.F)
                {
                    Console.WriteLine();
                }
            }
            void Conditions()
            {
                while (ConstantExpressionsAreExcluded.T)
                {
                    Console.WriteLine();
                }
                do
                {
                    Console.WriteLine();
                }
                while (ConstantExpressionsAreExcluded.F);
                var x = ConstantExpressionsAreExcluded.T ? 1 : 2;
            }
        }
    }

    public class GuardedTests
    {
        public void Guarded(string s1)
        {
            Guard1(s1);

            if (s1 == null)  // Noncompliant, always flse
            { // Secondary, this branch is never executed
            }
            else
            {
            }
        }

        public void Guard1<T>([ValidatedNotNull]T value) where T : class { }

        [AttributeUsage(AttributeTargets.Parameter)]
        public sealed class ValidatedNotNullAttribute : Attribute { }
    }

    public class CatchFinally
    {
        public void ObjectsShouldNotBeDisposedMoreThanOnce()
        {
            Stream stream = null;
            try
            {
                stream = File.Open("file", FileMode.Open);
                using (var reader = new StreamReader(stream))
                {
                    // read the file here

                    // StreamReader will dispose the stream automatically; set stream to null
                    // to prevent it from disposing twice (the recommended pattern for S3966)
                    stream = null;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        public void FalseNegatives()
        {
            // We cannot detect the case in ObjectsShouldNotBeDisposedMoreThanOnce method above
            // and to avoid False Positives we do not report in catch or finally
            object o = null;
            try
            {
            }
            catch
            {
                if (o != null) { } // False Negative S2583
                if (o == null) { } // False Negative S2589
            }
            finally
            {
                if (o != null) { } // False Negative S2583
                if (o == null) { } // False Negative S2589
            }
        }
    }

    class AsyncAwait
    {
        object _foo1;
        async Task Foo(Task t)
        {
            object o = null;
            _foo1 = o;
            await t; // awaiting clears the constraints
            if (_foo1 != null) { } // Compliant S2583
            if (_foo1 == null) { } // Compliant S2589
            if (o != null) { } // Noncompliant
            // Secondary@-1
            if (o == null) { } // Noncompliant
        }
    }

    public class StaticMethods
    {
        object _foo1;
        // https://github.com/SonarSource/sonar-csharp/issues/947
        void CallToMonitorWaitShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            System.Threading.Monitor.Wait(o); // This is a multi-threaded application, the fields could change
            if (_foo1 != null) { } // Compliant S2583
            if (_foo1 == null) { } // Compliant S2589
            if (o != null) { } // Noncompliant
            // Secondary@-1
            if (o == null) { } // Noncompliant
        }

        void CallToStaticMethodsShouldResetFieldConstraints()
        {
            object o = null;
            _foo1 = o;
            Console.WriteLine(); // This particular method has no side effects
            if (_foo1 != null) { } // False Negative S2583
            if (_foo1 == null) { } // False Negative S2589
            if (o != null) { } // Noncompliant
            // Secondary@-1
            if (o == null) { } // Noncompliant
        }
    }

    class FooContainer
    {
        public bool Foo { get; set; }
    }

    class TestNullConditional
    {
        void First(FooContainer fooContainer, bool bar)
        {
            if (fooContainer?.Foo == false || bar)
                Console.WriteLine(bar ? "1" : "2");
            else
                Console.WriteLine(fooContainer != null // Noncompliant FP (issue #1837)
                    ?
                    "3"
                    :
                    "4"); // Secondary
        }

        void Second(FooContainer fooContainer)
        {
            if (fooContainer?.Foo != true)
            {
                Console.WriteLine("3");
                if (fooContainer != null) // Noncompliant FP (issue #2164)
                {
                    Console.WriteLine("4");
                }
            }
        }
    }
}
