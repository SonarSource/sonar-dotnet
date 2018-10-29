using System;

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
            get
            {
                if (!this.initialized)
                {
                    throw new InvalidOperationException();
                }
                if (this.isDisposed)
                {
                    throw new ObjectDisposedException("object name");
                }

                return this.field2;  // Noncompliant
//                          ^^^^^^
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
                set { this.fieldb = value ; }   // Noncompliant
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
}
