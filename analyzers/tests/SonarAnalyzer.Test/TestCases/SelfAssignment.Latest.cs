class CSharp8
{
    void CoalescingAssignment()
    {
        int? value = null;
        value ??= value;
//      ^^^^^           Noncompliant
//                ^^^^^ Secondary@-1
    }
}

class WithPrimaryConstructorParams(int unused, int usedInInlineInitialization, int usedInMethod)
{
    int usedInInlineInitialization = usedInInlineInitialization;      // Compliant: param to field assignment

    WithPrimaryConstructorParams(int usedInMethod) : this(0, 0, usedInMethod)
    {
        this.usedInInlineInitialization = usedInInlineInitialization; // Compliant: field to field assignment
        usedInMethod = usedInMethod;                                  // Noncompliant: local to local assignment
                                                                      // Secondary@-1
    }

    void AssigningParamToParam()
    {
        usedInMethod = usedInMethod; // Noncompliant: param to param assignment (promoted to unspeakable field behind the scenes)
                                     // Secondary@-1
    }
}

class WithInlineArrays
{
    void Test(Buffer b)
    {
        b = b; // Noncompliant: local to local assignment
               // Secondary@-1
    }

    [System.Runtime.CompilerServices.InlineArray(10)]
    struct Buffer
    {
        int arrayItem;
    }
}

partial class PartialProperties
{
    public partial bool IsTrue { get; set; }

    void MyMethod()
    {
        IsTrue = IsTrue; // Noncompliant
                         // Secondary@-1
    }
}

class CSharp14
{
    int field;

    void NullConditionalAssignment(CSharp14 sample)
    {
        sample?.field = sample.field;   // FN NET-2779
        this?.field = this.field;       // FN NET-2779
    }

    void ExtensionProperties(CSharp14 sample)
    {
        this.Property = this.Property;      // Noncompliant
                                            // Secondary@-1
        sample.Property = sample.Property;  // Noncompliant
                                            // Secondary@-1
        sample.Property = this.Property;    // Compliant
        this.Property = sample.Property;    // Compliant
    }
}

static class Extensions
{
    extension(CSharp14 sample)
    {
        public int Property { get => 0; set { } }

        void ExtensionMethod()
        {
            sample.Property = sample.Property;  // Noncompliant
        }                                       // Secondary@-1
    }
}
