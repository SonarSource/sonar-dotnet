using System;
using System.Collections.Generic;
using System.Diagnostics;
using GalaSoft.MvvmLight;
using System.Runtime.CompilerServices;
using System.Windows;

class NonCompliantClass_FromRspec
{
    private int x;
    private int y;

    public int X
    {
        get { return x; }
        set { x = value; }
    }

    public int Y
    {
        get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
//                   ^
        set { x = value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
//            ^
    }
}

partial class NonCompliant_PartialClass
{
    private int x;
    private int y;
}

partial class NonCompliant_PartialClass
{
    public int Y
    {
        get { return x; }  // Noncompliant
        set { x = value; } // Noncompliant
    }
}

struct NonCompliantStruct_FromRspec
{
    private int x;
    private int y;

    public int X
    {
        get { return x; }
        set { x = value; }
    }

    public int Y
    {
        get { return x; }  // Noncompliant: field 'y' is not used in the return value
        set { x = value; } // Noncompliant: field 'y' is not updated
    }
}


class NonCompliant_UnderscoresInNamesAndCasing
{
    private int yyy;
    private int __x__X; // test that underscores and casing in names are ignored
    public int XX
    {
        get { return yyy; }   // Noncompliant {{Refactor this getter so that it actually refers to the field '__x__X'.}}
//                   ^^^
        set { yyy = value; }  // Noncompliant {{Refactor this setter so that it actually refers to the field '__x__X'.}}
//            ^^^
    }

    public int _Y___Y_Y_
    {
        get { return __x__X; } // Noncompliant
//                   ^^^^^^
    }
}

class NonCompliant_FieldTypeIsIgnored
{
    private int aaa;
    private string aString;

    public string AAA
    {
        get { return aString; } // Compliant - field called 'aaa' exists, but type is different
        set { aString = value; } // Compliant - Type is different.
    }
}

class NonCompliant_AssigningToExpression
{
    private int aaa;
    private string aString;

    public string AAA
    {
        set { aString = "foo" + value; } // Compliant field called 'aaa' exists, but type is different.
    }
}

partial class NonCompliant_PartialClass
{
    private object myProperty;
}
partial class NonCompliant_PartialClass
{
    private object anotherObject;
}
partial class NonCompliant_PartialClass
{
    public object MyProperty
    {
        get { return this.anotherObject; } // Noncompliant
        set { this.anotherObject = value; } // Noncompliant
    }
}


class NonCompliant_ComplexProperty
{
    private int field1;
    private int field2;
    private bool initialized;
    private bool isDisposed;

    public int Field1
    {
        get // Noncompliant {{Refactor this getter so that it actually refers to the field 'field1'.}}
//      ^^^
        {
            if (!this.initialized)
            {
                throw new InvalidOperationException();
            }
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("object name");
            }

            return this.field2;
        }

        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            this.field2 = value; // Noncompliant
//               ^^^^^^
        }
    }
}

class NonCompliant_Parentheses
{
    private int field1;
    private int field2;

    public int Field2
    {
        get { return (((this.field1))); } // Noncompliant
//                           ^^^^^^
        set
        {
            (((field1))) = value; // Noncompliant
//             ^^^^^^
        }
    }
}


class NonCompliant_OuterClass
{
    private string fielda;
    private string fieldb;

    struct NonCompliant_NestedClass
    {
        int fielda;
        int fieldb;

        public int FieldA
        {
            get { return this.fieldb; }     // Noncompliant
            set { this.fieldb = value; }   // Noncompliant
        }
    }
}


class Compliant_Indexer
{
    // Declare an array to store the data elements.
    private readonly int[] arr = new int[100];

    // Define the indexer to allow client code to use [] notation.
    public int this[int i]
    {
        get { return arr[i]; }  // Compliant - we don't know which field to check against
        set { arr[i] = value; }
    }
}

class CompliantClass
{
    private int xxx;

    public int XXX
    {
        get { return xxx; }
        set { xxx = value; }
    }

    public int UUU
    {
        get { return xxx; }     // Compliant - no matching field name
        set { xxx = value; }
    }

    private string _a_b_c;
    private string abc;
    private string yyy;

    public string Abc
    {
        get { return yyy; }     // Compliant - multiple possible matching field names, so don't raise
        set { yyy = value; }
    }
}

class Compliant_ImplicitProperties
{
    private string firstName;
    private string secondName;

    public string FirstName { get; set; } = "Jane";
    public string SecondName { get; set; }
}


class WrappedClass
{
    internal int field1;
    internal int field2;
}
class Compliant_WrappedObject
{
    private WrappedClass wrapped;

    public int Field2
    {
        get { return wrapped.field1; }
        set { wrapped.field1 = value; }
    }
}

class BaseClass
{
    protected int field1;
}

class ChildClass : BaseClass
{
    private int field2;

    public int Field1
    {
        get { return field2; }      // Noncompliant
        set { field2 = value; }     // Noncompliant
    }
}

public class Base
{
    protected long writeLockTimeout;
}

public class SubClass : Base
{
    public long WRITE_LOCK_TIMEOUT = 1000;
    public long WriteLockTimeout
    {
        get
        {
            return writeLockTimeout; // Compliant - points to the field in the base class
        }
        set
        {
            this.writeLockTimeout = value; // Compliant - points to the field in the base class
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8110
class Repro_8110
{
    class Base
    {
        bool _isSelected;

        public virtual bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }
    }

    class DerivedOnlyCallingBaseAccessors : Base
    {
        public override bool IsSelected
        {
            get => base.IsSelected;         // Noncompliant: FP, calls base behavior
            set => base.IsSelected = value; // Noncompliant: FP, calls base behavior
        }
    }

    class DerivedCallingBaseAccessorsAndAccessingUnexpectedField : Base
    {
        bool _anotherField;

        public override bool IsSelected
        {
            get { _ = base.IsSelected; return _anotherField; }      // Noncompliant
            set { base.IsSelected = value; _anotherField = value; } // Noncompliant
        }
    }

    class DerivedCallingBaseAccessorsAndAccessingExpectedField : Base
    {
        bool _isSelected; // Different from base._isSelected, which is not accessible

        public override bool IsSelected
        {
            get { _ = base.IsSelected; return _isSelected; }        // Compliant
            set { base.IsSelected = value; _isSelected = value; }   // Compliant
        }
    }
}

class MultipleOperations
{
    private int _foo;
    private int _bar;
    public int Foo
    {
        get
        {
            return _foo;
        }
        set
        {
            _bar = 1;
            _foo = value; // Compliant
            _bar = 0;
        }
    }
}

struct MultipleOperationsStruct
{
    private int _foo;
    private int _bar;
    public int Foo
    {
        get
        {
            return _foo;
        }
        set
        {
            _bar = 1;
            _foo = value; // Compliant
            _bar = 0;
        }
    }
}

// this usage is specific to MVVM Light framework
public class FooViewModel : ViewModelBase
{
    private int _foo;
    private int _bar;

    public int Foo
    {
        get => this._foo;
        set
        {
            if (this.Set(ref this._foo, 1)) // Compliant - the Set method does the assignment
            {
                this._bar = 1;
            }
        }
    }
}

public class FooViewModelWithoutSet : ViewModelBase
{
    private int _foo;
    private int _bar;

    public bool MySet(int x, int y) => true;

    public int Foo
    {
        get => this._foo;
        set
        {
            if (MySet(this._foo, 1))
            {
                this._bar = 1; // Noncompliant
            }
        }
    }
}

public class NoFieldUsage
{
    private int foo1;
    public int Foo1 { get; }

    private int foo2;
    public int Foo2 { get; set; }

    private int foo3;
    public int Foo3
    {
        get
        {
            Bar();
            return foo1; // Noncompliant
        }
        set // Noncompliant
//      ^^^
        {
            Bar();
        }
    }

    private int foo4;
    public int Foo4 => 4;

    private int foo5;
    public int Foo5
    {
        get // Noncompliant
//      ^^^
        {
            Bar();
            return 1;
        }
    }

    void Bar() { }
}

public class UpdateWithOut
{
    private int foo;
    public int Foo
    {
        set
        {
            Assign(value, out foo);
        }
    }
    private void Assign(int value, out int result)
    {
        result = value;
    }
}

public class UpdateWithOut2
{
    private int foo;
    private int bar;
    public int Foo
    {
        set
        {
            Assign(value, out bar); // Noncompliant
        }
    }
    private void Assign(int value, out int result)
    {
        result = value;
    }
}

public class MultipleStatements
{
    private string foo;
    private string bar;
    public string Foo
    {
        get
        {
            if (true)
            {
                throw new System.InvalidOperationException("");
            }
            foo = "stuff";
            return bar; // Noncompliant {{Refactor this getter so that it actually refers to the field 'foo'.}}
//                 ^^^
        }
        set
        {
            if (foo.Equals(foo))
            {
                bar = value; // Noncompliant {{Refactor this setter so that it actually refers to the field 'foo'.}}
//              ^^^
            }
        }
    }
    public string Bar
    {
        get
        {
            if (true)
            {
                throw new System.InvalidOperationException("");
            }
            this.bar = "stuff";
            return this.foo; // Noncompliant {{Refactor this getter so that it actually refers to the field 'bar'.}}
//                      ^^^
        }
        set
        {
            if (this.bar.Equals(foo))
            {
                this.foo = value; // Noncompliant {{Refactor this setter so that it actually refers to the field 'bar'.}}
//                   ^^^
            }
        }
    }

}

public class SpecialUsages
{
    private string _foo;

    public string Foo
    {
        get
        {
            var foo = _foo; // Compliant, field is read
            if (true)
            {
                foo += foo;
            }
            return foo;
        }
    }

    private object baz;
    public object Baz
    {
        get => baz ?? throw new System.InvalidOperationException(""); // Compliant
        set => baz = (baz == null) ? value : throw new System.InvalidOperationException(""); // Compliant
    }

    private int doNotSet;
    public int DoNotSet
    {
        set
        {
            throw new System.InvalidOperationException(""); // Compliant, if it throws do not raise
        }
    }

    private string tux;
    public string Tux
    {
        get
        {
            return tux + "salt"; // Compliant
        }
    }

    private string mux;
    public string Mux
    {
        get
        {
            return mux.Replace('x', 'y');
        }
    }

    private string MultipleFields
    {
        get => _foo == baz ? tux : mux;
    }

    private object baz2;
    public object Baz2
    {
        get /* 123 */ => /* 456 */ throw new System.InvalidOperationException(""); // Compliant
        set => throw new System.InvalidOperationException(""); // Compliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2867
public class Repro_2867
{
    private class ValueWrapper<T>
    {
        public T Value { get; set; }
        // ...
    }

    private readonly ValueWrapper<double> _someMember = new ValueWrapper<double>();

    public double SomeMember
    {
        get => _someMember.Value;
        set => _someMember.Value = value;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2435
public class CrossProcedural_Repro_2435
{
    private int expressionValue;
    private int expressionValueWrong;
    private int bodyValue;
    private int bodyValueWrong;
    private int tooNested;
    private int tooComplex;

    public int ExpressionValue
    {
        get => GetByExpression();
        set => SetByExpression(value);
    }

    public int ExpressionValue_ // With "this."
    {
        get => this.GetByExpression();
        set => this.SetByExpression(value);
    }

    public int ExpressionValueWrong
    {
        get => GetByExpression(); // Noncompliant
        set => SetByExpression(value); // Noncompliant
    }

    private int GetByExpression() => this.expressionValue;
    private void SetByExpression(int value) => this.expressionValue = value;

    public int BodyValue
    {
        get
        {
            return GetByBody();
        }
        set
        {
            SetByBody(value);
        }
    }

    public int BodyValue_ // With "this."
    {
        get
        {
            return this.GetByBody();
        }
        set
        {
            this.SetByBody(value);
        }
    }

    public int BodyValue__ // get/set with more than one statement
    {
        get // Noncompliant, only one function invocation is supported
        {
            try
            {
                var nothing = IrrelevantFunction();
                return GetByBody();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        set // Noncompliant, only one function invocation is supported
        {
            try
            {
                IrrelevantProcedure(value);
                SetByBody(value);
            }
            catch (Exception ex)
            {
                // Nothing
            }
        }
    }

    public int BodyValueWrong
    {
        get // Noncompliant
        {
            return this.GetByExpression();
        }
        set // Noncompliant
        {
            this.SetByExpression(value);
        }
    }

    private int IrrelevantFunction()
    {
        return 42; // Do not return local fields
    }

    private void IrrelevantProcedure(int value)
    {
        // Do not set local fields
    }

    private int GetByBody()
    {
        return this.bodyValue;
    }

    private void SetByBody(int value)
    {
        this.bodyValue = value;
    }

    public int TooNested
    {
        get => GetTooNestedA(); // Noncompliant, only one level of nesting is supported
        set => SetTooNestedA(value); // Noncompliant, only one level of nesting is supported
    }

    private int GetTooNestedA() => GetTooNestedB();
    private void SetTooNestedA(int value) => SetTooNestedB(value);

    private int GetTooNestedB() => this.tooNested;
    private void SetTooNestedB(int value) => this.tooNested = value;

    public int TooComplex
    {
        get // Noncompliant, only single return scenario is supported
        {
            if (true)
                return GetTooComplex();
            else
                return GetTooComplex();
        }
        set // Noncompliant, only one function invocation is supported
        {
            if (true)
                SetTooComplex(value);
            else
                SetTooComplex(value);

        }
    }

    private int GetTooComplex() => this.tooComplex;
    private void SetTooComplex(int value) => this.tooComplex = value;

    public int WithAttributeWithoutBody
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private set;
    }

    public int ReadOnlyProperty
    {
        [DebuggerStepThrough]
        get;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3441
public class Repro3441
{
    private readonly List<int> _data = new List<int>();
    public List<int> Data
    {
        get => _data;
        // The setter in this case is only executed by a deserializer, but we don’t want to replace the field instance.
        private set // Noncompliant
        {
            foreach (var item in value)
            {
                _data.Add(item);
            }
        }
    }
}

// See: https://github.com/SonarSource/sonar-dotnet/issues/4339
public class TestCases
{
    bool pause;
    public bool Pause
    {
        get => pause;
        set => pause |= value;
    }

    private int textBufferUndoHistory;
    public int TextBufferUndoHistory
    {
        get
        {
            return textBufferUndoHistory = GetValue();
        }
    }
    private static int GetValue() => 1;

    // https://github.com/SonarSource/sonar-dotnet/issues/5259
    private int[] attributes;
    public int[] Attributes
    {
        set { value.CopyTo(attributes, 0); } // Noncompliant - FP
    }

    private const string PREFIX = "pre";
    private string m_prefix;
    public string Prefix
    {
        get { return m_prefix; } // Compliant PREFIX is const
        set { m_prefix = value; } // Compliant PREFIX is const
    }
}

public class ContainsConstraint
{
    private bool _ignoreCase;

    public ContainsConstraint IgnoreCase
    {
        get // Compliant - _IgnoreCase has different type
        {
            _ignoreCase = true;
            return this;
        }
    }
}

public class AClass
{
    public static long WRITE_LOCK_TIMEOUT = 1000;
    public long longValue;
    public long WriteLockTimeout
    {
        get
        {
            return longValue; // Compliant - WRITE_LOCK_TIMEOUT is static field when the property is not static.
        }
        set
        {
            longValue = value; // Compliant - WRITE_LOCK_TIMEOUT is static field when the property is not static.
        }
    }

    public static long TEST_STATIC_CASE = 1000;
    public static long ALong = 2000;
    public static long TestStaticCase
    {
        get
        {
            return ALong; // Noncompliant
        }
        set
        {
            ALong = value; // Noncompliant
        }
    }
}
