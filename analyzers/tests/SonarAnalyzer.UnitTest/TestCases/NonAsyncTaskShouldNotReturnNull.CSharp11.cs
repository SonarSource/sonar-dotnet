using System.Threading.Tasks;

public interface IMath
{
    public static virtual Task<object> GetValue()
    {
        return null; // Noncompliant
    }
}
