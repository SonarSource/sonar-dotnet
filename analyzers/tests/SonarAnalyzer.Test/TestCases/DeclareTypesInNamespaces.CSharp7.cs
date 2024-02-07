public interface Int // Noncompliant {{Move 'Int' into a named namespace.}}
{
    // Interface can't have nested types, we want to report only on the outer struct
    interface InnerInt // Error [CS8107] Feature 'default interface implementation' is not available in C# 7
    {
    }
}
