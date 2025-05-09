
public class Sample
{
    public int PublicField;
    private int PrivateField;
    internal int InternalField;
    file int FileField; // Error [CS0106]
    protected const int ConstProtectedField = 42;

    abstract protected readonly int AbstractReadonlyProtectedField; // Error [CS0681]
    protected readonly int ReadonlyProtectedField;              // Noncompliant

    protected readonly int ReadonlyProtectedField1, ReadonlyProtectedField2, ReadonlyProtectedField3;
    //                     ^^^^^^^^^^^^^^^^^^^^^^^                                                      Noncompliant
    //                                              ^^^^^^^^^^^^^^^^^^^^^^^                             Noncompliant@-1
    //                                                                       ^^^^^^^^^^^^^^^^^^^^^^^    Noncompliant@-2

    protected int ProtectedField;                               // Raise by SA1306
    private protected int PrivateProtectedField;                // Raise by SA1306
    protected static int StaticProtectedField;                  // Raise by SA1306
    private protected static int StaticPrivateProtectedField;   // Raise by SA1306
    protected int MultipleProtectedField1, MultipleProtectedField2, MultipleProtectedField3; // Raise by SA1306

    protected static readonly int StaticReadonlyProtectedField; // Conflict with SA1311
    protected internal int ProtectedInternalField;              // Conflict with SA1307
    protected internal static int StaticProtectedInternalField; // Conflict with SA1307

    public int publicField;
    private int privateField;
    internal int internalField;
    file int fileField; // Error [CS0106]
    protected int protectedField;
    protected int multipleProtectedField1, multipleProtectedField2, multipleProtectedField3;
    protected internal int protectedInternalField;
    private protected int privateProtectedField;
    protected static int staticProtectedField;
    protected readonly int readonlyProtectedField;
    protected static readonly int staticReadonlyProtectedField;
    protected internal static int staticProtectedInternalField;
    private protected static int staticPrivateProtectedField;
}
