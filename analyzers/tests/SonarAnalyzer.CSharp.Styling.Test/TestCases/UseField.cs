public class Sample
{
    private int field;

    private int Private { get; set; }                       // Noncompliant {{Use field instead of this private auto-property.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    protected int Protected { get; set; }                   // Noncompliant {{Use field instead of this protected auto-property.}}
    protected static int ProtectedStatic { get; set; }      // Noncompliant {{Use field instead of this protected auto-property.}}
    private protected int PrivateProtected { get; set; }    // Noncompliant {{Use field instead of this private protected auto-property.}}
    private int PrivateReadOnly { get; }                    // Noncompliant
    protected int ProtectedReadOnly { get; }                // Noncompliant
    protected static int ProtectedStaticReadOnly { get; }   // Noncompliant
    private protected int PrivateProtectedReadOnly { get; } // Noncompliant

    // Compliant
    public int Public { get; set; }
    internal int Internal { get; set; }
    internal protected int InternalProtected { get; set; }
    protected virtual int ProtectedVirtual { get; set; }

    private int WithArrow
    {
        get => field;
        set => field = value;
    }

    private int WithBody
    {
        get
        {
            return field;
        }
        set
        {
            field = value;
        }
    }
}

public abstract class Abstract
{
    protected abstract bool IsAbstract { get; set; }
}

public abstract class Override : Abstract
{
    protected override bool IsAbstract { get; set; }
}
