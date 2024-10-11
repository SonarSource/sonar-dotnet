int x = 42;

  (x, var y) = (x, 42);
// ^                    Noncompliant
//              ^       Secondary@-1

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
    public partial bool IsTrue
    {
        get => IsTrue;
        set => IsTrue = value;
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
