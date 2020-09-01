
namespace Tests
{
    class NullCoalesceAssignment
    {
        private object x;
        private object y;

        public object X
        {
            get { return x; }
            set { x ??= value; }
        }

        public object Y
        {
            get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
//                       ^
            set { x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
//                ^
        }
    }

    class NullCoalesceAssignment_SubExpression
    {
        private object x;
        private object y;

        public object X
        {
            get { return x; }
            set { var something = x ??= value; }
        }

        public object Y
        {
            get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
            set { var y = x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
        }
    }
}
