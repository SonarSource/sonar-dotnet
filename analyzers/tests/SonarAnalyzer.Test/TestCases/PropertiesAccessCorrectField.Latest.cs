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

class NullConditonalAssignment
{
    public class Sample
    {
        public int Value { get; set; }
    }

    private Sample correctField;
    private Sample wrongField;

    public Sample CorrectField
    {
        get;    // Noncompliant
        set     // Noncompliant
        {
            wrongField?.Value = value.Value;
        }
    }
}

class ExpressionBodiedProperties
{
    private int x;
    private int y;
    private int z;
    private int s;

    public int X => x;                                              // Compliant - field called 'x' exists and is referred to
    public int Y => x;                                              // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}
    public int Z => 10*x;                                           // Noncompliant {{Refactor this getter so that it actually refers to the field 'z'.}}
    public string S => $"{x}";                                      // Compliant - field called 's' exists, but type is different
    public int A => x + y;                                          // Compliant - field called 'a' does not exist
    public int E => throw new System.InvalidOperationException();   // Compliant - if it only throws, do not raise an issue
}

// https://sonarsource.atlassian.net/browse/NET-2812
class FieldKeyword
{
    private int y;
    public int Y => field; // Noncompliant {{Refactor this getter so that it actually refers to the field 'y'.}}

    public int Field => field;

    class FieldKeywordEscaped
    {
        public int field;
        public int something;
        public int Field => @field;
        public int Something => @field; // Noncompliant {{Refactor this getter so that it actually refers to the field 'something'.}} 
        public int Other => @field;
    }
}

class HelperFollowedFromSingleReturnPath
{
    private int field;
    private int field1;
    private int otherField;

    private int GetField1() => field1;
    private int GetOtherField() => otherField;

    public int Field1 => this.GetField1(); // Compliant - expression body resolves to a single invocation; helper is followed and returns the matching field

    public int OtherField
    {
        get // Compliant - helper returns the matching field
        {
            return this.GetOtherField();
        }
    }

    public int Field
    {
        get // Noncompliant {{Refactor this getter so that it actually refers to the field 'field'.}} - helper returns 'otherField'
        {
            return this.GetOtherField();
        }
    }
}

class HelperNotFollowedFromMultipleReturnPaths
{
    private int field;
    private int field2;
    private int field3;
    private int? field4;

    private int GetField() => field;
    private int GetField2() => field2;
    private int GetField3() => field3;
    private int? GetField4() => field4;

    public int Field2 => true ? this.GetField2() : this.GetField2(); // Noncompliant - indirect access through a helper is recognized only when the getter has a single return path

    public int Field3 => true switch // Noncompliant - indirect access through a helper is recognized only when the getter has a single return path
    {
        true => this.GetField3(),
        false => this.GetField3(),
    };

    public int? Field4 => this.GetField4() ?? this.GetField4(); // Noncompliant - indirect access through a helper is recognized only when the getter has a single return path

    public int Field
    {
        get // Noncompliant - indirect access through a helper is recognized only when the getter has a single return path
        {
            if (true)
                return this.GetField();
            else
                return this.GetField();
        }
    }
}
