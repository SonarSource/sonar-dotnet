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

public static class Extensions
{
    extension(string s)
    {
        public Task<object> NonCompliantInstanceProp => null;                                       // Noncompliant
        public Task<object> CompliantInstanceProp => new Task<object>(() => new object());          // Compliant

        public Task<object> NonCompliantInstanceMethod() => null;                                   // Noncompliant
        public Task<object> CompliantInstanceMethod() => new Task<object>(() => new object());      // Compliant

        public static Task<object> NonCompliantStaticProp => null;                                  // Noncompliant
        public static Task<object> CompliantStaticProp => new Task<object>(() => new object());     // Compliant

        public static Task<object> NonCompliantStaticMethod() => null;                              // Noncompliant
        public static Task<object> CompliantStaticMethod() => new Task<object>(() => new object()); // Compliant
    }
}
