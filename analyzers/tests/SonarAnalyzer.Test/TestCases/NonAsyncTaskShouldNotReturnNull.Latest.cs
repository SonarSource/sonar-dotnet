using System.Threading.Tasks;

public interface IMath
{
    public static virtual Task<object> GetValue()
    {
        return null; // Noncompliant
    }
}

public partial class PartialProperties
{
    public partial Task<object> Prop1 { get; }
    public partial Task<object> Prop2 { get; }
}
