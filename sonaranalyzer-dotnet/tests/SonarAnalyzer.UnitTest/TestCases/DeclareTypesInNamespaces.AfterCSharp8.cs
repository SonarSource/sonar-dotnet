public interface Int // Noncompliant {{Move 'Int' into a named namespace.}}
{
    interface InnerInt // Compliant - starting with C# 8 interface can host types and members
    {
    }
}
