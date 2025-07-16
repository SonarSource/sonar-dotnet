namespace CSharp8
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
            get { return x; }            // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
            set { var y = x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
        }
    }
}

namespace CSharp9
{
    public record Record
    {
        private object w;
        private object x;
        private object y;
        private object z;

        public object X
        {
            get { return x; }
            set { x ??= value; }
        }

        public object W
        {
            get { return w; }
            init { w ??= value; }
        }

        public object Y
        {
            get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
    //                   ^
            set { x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'y'.}}
    //            ^
        }

        public object Z
        {
            get { return x; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'z'.}}
    //                   ^
            init { x ??= value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'z'.}}
    //             ^
        }
    }
}

namespace CSharp12
{
    // https://github.com/SonarSource/sonar-dotnet/issues/8101
        public class SomeClass(object y)
        {
            object x;

            public object Y
            {
                get { return x; }    // FN
                set { x ??= value; } // FN
            }
        }
}

// https://sonarsource.atlassian.net/browse/NET-429
namespace CSharp13
{
    public partial class PartialProperties
    {
        public partial int X
        {
            get { return y; }  // Noncompliant {{Refactor this getter so that it actually refers to the field 'x'.}}
            set { y = value; } // Noncompliant {{Refactor this setter so that it actually refers to the field 'x'.}}
        }

        public partial int Y { get; set; }
    }
}
