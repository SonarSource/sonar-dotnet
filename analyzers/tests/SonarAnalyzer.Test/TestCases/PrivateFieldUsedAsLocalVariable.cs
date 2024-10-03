using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class PrivateFieldUsedAsLocalVariable
    {
        private int F0 = 0; // Compliant - unused

        private int F1 = 0; // Noncompliant {{Remove the field 'F1' and declare it as a local variable in the relevant methods.}}
//                  ^^^^^^
        public int F2 = 0; // Compliant - Public

        void Use(int foo) { }

        void M1()
        {
            ((F1)) = 42;
            Use(F1);
            F2 = 42;
        }

        private int F3 = 1; // Compliant - Referenced from another field initializer
        public int F4 = F3; // Compliant - Public // Error [CS0236]

        private int F5 = 0; // Noncompliant
        private int F6; // Noncompliant

        void M2()
        {
            F5 = 42;
            Use(F5);
            F6 = 42;
            Use(F6);
        }

        private int F7 = 0; // Compliant - Read before write
        void M3()
        {
            Console.WriteLine(F7);
            F7 = 42;
        }

        private int F8 = 0; // Compliant, False Negative, we cannot detect if a field is both updated and read in the same statement
        private int F9 = 0; // Compliant, False Negative, we cannot detect if statements that set the field in both branches
        private int F10 = 0; // Compliant - not assigned from every path
        void M4(bool p1)
        {
            for (F8 = 0; F8 < 42; F8++)
            { }

            if (p1)
            {
                F9 = 0;
                F10 = 0;
            }
            else
            {
                F9 = 1;
            }
            Console.WriteLine(F9);
            Console.WriteLine(F10);
        }

        private int F11 = 0; // Compliant - first read through 'this.'
        private int F12 = 0; // Noncompliant
        void M5()
        {
            Console.WriteLine(this.F11);
            F11 = 42;

            this.F12 = 42;
            Console.WriteLine(F12);
        }

        private int F13 = 0; // Compliant - parameter of same name is assigned, not the field
        private int F14 = 0; // Noncompliant
        void M6(int F13, int F14)
        {
            F13 = 42;
            this.F14 = 42;
            Use(this.F14);
        }

        private int F15 = 0; // Compliant - returned in property getter
        private int F16 = 0; // Noncompliant, property is assigning first
        public int P1 { get { return F15; } }
        public int P2 { get { F16 = 42; return F16; } }

        private static int F17 = 0; // Compliant
        private int F18 = 0; // Compliant
        public int P3 { get; set; } = F17;
        public int M7() => F18;
        void M8()
        {
            F17 = 42;
            F18 = 42;
        }

        private int F19 = 42; // Compliant - accessed through instance
        private int F20 = 42; // Compliant - accessed through instance
        private int F21 = 42; // Compliant - accessed through instance
        private static int F22 = 42; // Noncompliant, overwritten static instance
        private int F23 = 42;
        void M9(PrivateFieldUsedAsLocalVariable inst)
        {
            F19 = inst.F19;

            if (inst.F20 == 42)
            {
                Console.WriteLine();
            }

            F20 = 0;

            inst.F21 = 42;
            if (F21 == 42)
            {
                Console.WriteLine();
            }

            PrivateFieldUsedAsLocalVariable.F22 = 42;
            Console.WriteLine(F22);

            this.F21 = this?.F23 ?? 42;
        }

        private int F24 = 42; // Noncompliant - passed as 'out'
        private int F25 = 42; // Compliant - passed as 'ref'
        void M10()
        {
            M11(out F24, ref F25);
            Use(F24);
            Use(F25);
        }

        void M11(out int a, ref int b)
        {
            a = 42;
            b = 42;
        }

        private int F26 = 42; // Noncompliant - always assigned from constructor
        private int F27 = 42; // Compliant - passed to another constructor
        PrivateFieldUsedAsLocalVariable() : this(F27) // Error [CS0120]
        {
            F26 = 42;
            Use(F26);
        }

        PrivateFieldUsedAsLocalVariable(int a)
        {
        }

        private int F28 = 42; // Noncompliant - always assigned from event
        private int F29 = 42; // Compliant
        event EventHandler E1
        {
            add
            {
                F28 = 42;
                F29 = 42;
                Use(F28); // use after assignment in event
            }
            remove
            {
                Console.WriteLine(F29);
            }
        }

        private int F30 = 42; // Noncompliant - always assigned
        private int F31 = 42; // Compliant - read in a different method
        void M12()
        {
            F30 = 42;
            Use(F30);
            F31 = 40;
        }

        private string F32 = ""; // Noncompliant
        void M13()
        {
            this.F32 = 42.ToString();
            Console.WriteLine(this.F32);

            Console.WriteLine(this.F31);
        }

        private static string F33 = ""; // Noncompliant
        ~PrivateFieldUsedAsLocalVariable()
        {
            F33 = "foo";
            Console.WriteLine(F33);
        }

        private string F34 = ""; // Compliant
        public static PrivateFieldUsedAsLocalVariable operator +(PrivateFieldUsedAsLocalVariable c1, PrivateFieldUsedAsLocalVariable c2)
        {
            PrivateFieldUsedAsLocalVariable.F33 = "foo";
            c1.F34 = "foo";
            return null;
        }

        [Obsolete]
        private string F35 = ""; // Compliant, even though it is overwritten, the field has attribute and is not reported
        void M14()
        {
            this.F35 = "foo";
        }

        private int F36; // Should be raised by S4487
        public void M15(int i) => F36 = i + 1;

        private string F37; // Compliant
        protected string SomeString
        {
            get { return F37 ?? (F37 = "5"); }
        }

        private int InvocationWithSideEffect = 0;
        private int PropertyWithSideEffect = 0; // Noncompliant FP

        public int SetValueInProperty { set { PropertyWithSideEffect = 42; } }

        void MethodWithSideEffect()
        {
            InvocationWithSideEffect = 42;
        }

        void M16()
        {
            InvocationWithSideEffect = 5;
            MethodWithSideEffect();
            Use(InvocationWithSideEffect);
        }

        void M17()
        {
            PropertyWithSideEffect = 5;
            SetValueInProperty = 42;
            Use(PropertyWithSideEffect);
        }

        private int F38 = 21; // Compliant - unread

        void M18()
        {
            F38 = 42;
        }

        struct SomeStruct
        {
            public int Field;
        }

        private SomeStruct F39; // Noncompliant
        private SomeStruct? F40 = null; // Noncompliant

        void M19()
        {
            F39 = new SomeStruct() { Field = 42 };
            F39.Field = 42;
        }

        void M20()
        {
            F40 = new SomeStruct() { Field = 42 };
            if (F40 != null)
            {
                Console.WriteLine(F40?.Field);
            }
        }
    }

    public partial class SomePartialClass
    {
        private int F0 = 0; // Compliant - partial classes are not checked

        void M1()
        {
            F0 = 0;
            Console.WriteLine(F0);
        }
    }

    public struct SomeStruct
    {
        private int F0; // Noncompliant

        void M1()
        {
            F0 = 0;
            Console.WriteLine(F0);
        }
    }

    public class AccessInExpressionBodiedConstructs
    {
        private string F0;
        public string M0 => F0 ?? (F0 = "test");
        private string F1;
        public string M1() => F1 ?? (F1 = "test");
        private string F2;
        public string M2
        {
            get => F2 ?? (F2 = "test");
            set => F2 = "value";
        }
        private string F3;
        AccessInExpressionBodiedConstructs()
        {
            F3 = F3 ?? "test";
        }
        private string F4;
        ~AccessInExpressionBodiedConstructs()
        {
            F4 = F4 ?? "test";
        }
        private string F5;
        public string this[int i]
        {
            get => "value";
            set => F5 = F5 ?? "test";
        }
    }

    public struct CompoundAssignmentReadAsWellAsWrite
    {
        private int F0;
        void M1()
        {
            F0 += 1;
            Console.WriteLine(F0);
        }
    }

    // Repro https://github.com/SonarSource/sonar-dotnet/issues/9672
    public class MethodUpdateTest
    {
        private int value = 0; // Noncompliant FP

        private void Reset() => value = 0;

        public void TestMethod()
        {
            value = 20;

            while (value != 0)
            {
                Reset();
            }
        }
    }

    // As S4487 will raise when a private field is written and not read, S1450 won't raise on these cases
    // These tests where finding issues before with S1450 and should find them with S4487 now
    public class TestsForS4487Harmony
    {
        private int F1 = 0; // compliant

        public void M1()
        {
            ((F1)) = 42;
        }

        private int F5 = 0; // compliant
        private int F6; // compliant
        public void M2()
        {
            F5 = 42;
            F6 = 42;
        }

        private int F14 = 0; // compliant
        public void M6(int F14)
        {
            this.F14 = 42;
        }
        private int F28 = 42; // compliant
        public event EventHandler E1
        {
            add
            {
                F28 = 42;
            }
            remove
            {
            }
        }

        private int F36; // compliant
        public void M15(int i) => F36 = i + 1;
    }

    public class Broker
    {
        public event EventHandler Receive;
        public void Process() { Receive?.Invoke(this, EventArgs.Empty); }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/8239
    public class Repro_8239
    {
        private bool _received; // Noncompliant

        public void Program(Broker broker)
        {
            broker.Receive += Broker_Receive; // Broker_Receive should be treated as "public" as it is passed as a delegate
            _received = false; // This is not compliant because this line is after registering the event
            broker.Process();

            if (_received)
            {
                Console.WriteLine("OK");
            }
        }

        private void Broker_Receive(object sender, EventArgs e)
        {
            _received = true;
        }
    }

    public class Repro_8239_Compliant
    {
        private bool _received;

        public void Program(Broker broker)
        {
            _received = false;
            broker.Receive += Broker_Receive;
            broker.Process();

            if (_received)
            {
                Console.WriteLine("OK");
            }
        }

        private void Broker_Receive(object sender, EventArgs e)
        {
            _received = true;
        }
    }

    public class Repo_8239_Variation
    {
        private bool _wasCalled;

        public void Program()
        {
            var list = new List<int>();
            _wasCalled = false;

            list.ForEach(Increment);            // Increment is passed as a delegate `new Action<int>(Increment)` and should be treated as "public"

            if (_wasCalled)
            {
                Console.WriteLine("OK");
            }
        }

        private void Increment(int dummy)
            => _wasCalled = true;
    }

    public class Repo_8239_Variation_Noncompliant
    {
        private bool _wasCalled; // Noncompliant

        public void Program()
        {
            var list = new List<int>();
            list.ForEach(x => Increment(x));
            _wasCalled = false;

            if (_wasCalled)
            {
                Console.WriteLine("OK");
            }
        }

        private void Increment(int dummy) =>
            _wasCalled = true;
    }
}
