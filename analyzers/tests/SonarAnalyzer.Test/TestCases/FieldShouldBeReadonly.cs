using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class Mod
    {
        public static void DoSomething(ref int x)
        {
        }
        public static void DoSomething2(out int x)
        {
            x = 6;
        }
    }

    class Person
    {
        private int _birthYear;  // Noncompliant {{Make '_birthYear' 'readonly'.}}
//                  ^^^^^^^^^^
        int _birthMonth = 3;  // Noncompliant
        int _birthDay = 31;  // Compliant, the setter action references it
        int _birthDay2 = 31;  // Compliant, it is used in a delegate
        int _birthDay3 = 31;  // Compliant, it is passed as ref outside the ctor
        int _birthDay4 = 31;  // Compliant, it is passed as out outside the ctor
        int _legSize = 3;
        int _legSize2 = 3;
        int _handSize = 3;
        int _handSize2 = 3;
        int _neverUsed;

        private readonly Action<int> setter;

        Person(int birthYear)
        {
            setter = i => { _birthDay = i; };

            System.Threading.Thread t1 = new System.Threading.Thread
                (delegate()
                {
                    _birthDay2 = 42;
                });
            t1.Start();

            _birthYear = birthYear;
        }

        private void M()
        {
            Mod.DoSomething(ref this._birthDay3);
            Mod.DoSomething2(out _birthDay4);
        }

        public int LegSize
        {
            get
            {
                _legSize2++;
                return _legSize;
            }
            set { _legSize = value; }
        }

        public int HandSize
        {
            get
            {
                --_handSize2;
                return _handSize;
            }
            set { _handSize = value; }
        }
    }

    partial class Partial
    {
        private int i; // Non-compliant, but not reported now because of the partial
    }
    partial class Partial
    {
        public Partial()
        {
            i = 42;
        }
    }

    class X
    {
        private int x; // Compliant
        private int y; // Compliant
        private int z = 10; // Noncompliant
        private int w = 10; // Noncompliant
        private int a, b; // Noncompliant
//                           Noncompliant@-1
        public X()
        {
            new X().x = 12;
            var xx = new X();
            Modif(ref xx.y);

            Modif(ref (z));
            this.w = 42;
            a = 42;
            b = 42;
        }

        private void Modif(ref int i) { }
    }

    struct X1Struct
    {
        public Y1 y;
    }
    class X1Class
    {
        public Y1 y;
    }
    struct Y1
    {
        public string z;
    }

    class MyClass
    {
        private X1Struct x; // Compliant
        // See https://github.com/SonarSource/sonar-dotnet/issues/2291
        private X1Struct y; // Compliant - could be set as readonly but this changes the behavior of the field
        private IntPtr myPtr; // Noncompliant

        private X1Class z; // Noncompliant

        private bool field = false;

        public MyClass()
        {
            x = new X1Struct();
            y = new X1Struct();
            z = new X1Class();
            myPtr = new IntPtr(12);
            (this.y.y).z = "a";
            (this.z.y).z = "a";
            if (this.field)
            { }
        }

        public void M()
        {
            (this.x.y).z = "a";
            (this.z.y).z = "a";
        }

        private class Nested
        {
            private readonly MyClass inst;
            public Nested()
            {
                inst = new MyClass();
            }
            private void Method()
            {
                this.inst.field = false;
            }
        }
    }

    class Attributed
    {
        [My]
        private int myField1; // Compliant because of the attribute

        public Attributed()
        {
            myField1 = 42;
        }
    }

    public class MyAttribute : Attribute { }

    // See https://github.com/SonarSource/sonar-dotnet/issues/1009
    // Issue with leading trivia not moved to the readonly modifier
    public class MyClassWithField
    {
        #region Test
        string teststring = null; // Noncompliant
        #endregion

        public void Do()
        {
            var x = teststring;
        }
    }

    [Serializable]
    public class SerializableClass
    {
        private int field; // Compliant, containing class is marked with [Serializable]

        public SerializableClass()
        {
            field = 5;
        }
    }

    public class DerivedFromSerializable : SerializableClass
    {
        private int otherField; // Noncompliant, Serializable attribute is not inherited

        public DerivedFromSerializable()
        {
            otherField = 5;
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3339
    public class NullCoalesceAssignment
    {
        private string value = null;

        public void DoSomething()
        {
            value ??= "Empty";
        }
    }

    public class TupleExpressionAssignment
    {
        private string a; // Compliant - set in tuple expression
        private string b; // Compliant - set in tuple expression
        private X1Struct x1; // Compliant - set in tuple expression
        private X1Struct x2; // Compliant - set in tuple expression

        public TupleExpressionAssignment()
        {
            a = string.Empty;
            b = string.Empty;
            x1 = new X1Struct();
            x2 = new X1Struct();
        }

        public void SomeMethod()
        {
            (a, b) = NewValues();
            ((this.x1.y).z, (this.x2.y).z) = NewValues();
        }

        private (string, string) NewValues() => ("FOO", "Bar");
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/9657
    public struct StructWithThisReassignmentToDefaultInMethod
    {
        private int number;     // Compliant: the field's value is overwritten in the Reset() method (with the 'this = default' assignment)
        public int Number => number;

        public StructWithThisReassignmentToDefaultInMethod(int number)
        {
            this.number = number;
        }

        public void Reset()
        {
            this = default;
        }
    }

    public struct StructWithThisReassignmentToNewInMethod
    {
        private int number;     // Compliant: the field's value is overwritten in the Reassign method
        public int Number => number;

        public StructWithThisReassignmentToNewInMethod(int number)
        {
            this.number = number;
        }

        public void ReAssign()
        {
            this = new StructWithThisReassignmentToNewInMethod(42);
        }
    }

    public struct StructWithThisReassignmentInConstructor
    {
        private int number;     // Noncompliant - assignment to this happens only in the constructor
        public int Number => number;

        public StructWithThisReassignmentInConstructor(int number)
        {
            this = default;
            this.number = number;
        }
    }

    public class NestedStructWithThisReassignment
    {
        private int number;     // Noncompliant - assignment to this happens only in the nested type
        public int Number => number;

        public NestedStructWithThisReassignment(int number)
        {
            this.number = number;
        }

        public struct NestedStruct
        {
            private int number;
            public int Number => number;

            public NestedStruct(int number)
            {
                this.number = number;
            }

            public void Reset()
            {
                this = default;
            }
        }
    }
}

// https://sonarsource.atlassian.net/browse/NET-691
namespace Repro_NET691
{
    public class MyClass
    {
        private bool myField = true; // Noncompliant FP

        public void ToggleField()
        {
            myField.Toggle();
        }
    }

    public static class BoolExtension
    {
        public static void Toggle(this ref bool value)
        {
            value = !value;
        }
    }
}
