public interface Int // Noncompliant {{Move 'Int' into a named namespace.}}
{
    interface InnerInt // Error [CS0524] - interface can't host types | we want to report only on the outer struct
    {
    }
}
