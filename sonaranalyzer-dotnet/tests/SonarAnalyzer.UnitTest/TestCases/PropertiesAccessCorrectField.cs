using System;
using GalaSoft.MvvmLight;

namespace Tests.Diagnostics
{
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
//                       ^
            set { x = value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
//                ^
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
//                       ^^^
            set { yyy = value; }  // Noncompliant {{Refactor this setter so that it actually refers to the field '__x__X'.}}
//                ^^^
        }

        public int _Y___Y_Y_
        {
            get { return __x__X; } // Noncompliant
//                       ^^^^^^
        }
    }

    class NonCompliant_FieldTypeIsIgnored
    {
        private int aaa;
        private string aString;

        public string AAA
        {
            get { return aString; } // Noncompliant - field called 'aaa' exists, even though type is different
//                       ^^^^^^^
            set { aString = value; } // Noncompliant
//                ^^^^^^^
        }
    }

    class NonCompliant_AssigningToExpression
    {
        private int aaa;
        private string aString;

        public string AAA
        {
            set { aString = "foo" + value; } // Noncompliant
//                ^^^^^^^
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
//          ^^^
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
//                   ^^^^^^
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
//                               ^^^^^^
            set
            {
                (((field1))) = value; // Noncompliant
//                 ^^^^^^
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
            get { return field2; }      // Compliant - aren't checking inherited fields
            set { field2 = value; }     // Compliant
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
//          ^^^
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
//          ^^^
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
//                     ^^^
            }
            set
            {
                if (foo.Equals(foo))
                {
                    bar = value; // Noncompliant {{Refactor this setter so that it actually refers to the field 'foo'.}}
//                  ^^^
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
//                          ^^^
            }
            set
            {
                if (this.bar.Equals(foo))
                {
                    this.foo = value; // Noncompliant {{Refactor this setter so that it actually refers to the field 'bar'.}}
//                       ^^^
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
}
