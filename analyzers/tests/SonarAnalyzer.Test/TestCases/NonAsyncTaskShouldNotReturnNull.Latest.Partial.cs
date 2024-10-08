using System.Threading.Tasks;

public partial class PartialProperties
{
    public partial Task<object> Prop1 => null; // Noncompliant

    public partial Task<object> Prop2
    {
        get
        {
            return null; // Noncompliant
        }
    }
}
