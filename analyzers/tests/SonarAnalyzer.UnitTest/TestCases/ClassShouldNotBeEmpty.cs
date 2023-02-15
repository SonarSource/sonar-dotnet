
class Empty { }                              // Noncompliant {{Remove this empty class, or add members to it.}}
//    ^^^^^

public class PublicEmpty { }                 // Noncompliant
internal class InternalEmpty { }             // Noncompliant

class EmptyWithComments                      // Noncompliant
{
    // Some comment
}

class NotEmpty
{
    public int SomeProperty => 42;
}

class OuterClass
{
    class InnerEmpty1 { }                    // Noncompliant
    private class InnerEmpty2 { }            // Noncompliant
    protected class InnerEmpty3 { }          // Noncompliant
    internal class InnerEmpty4 { }           // Noncompliant
    protected internal class InnerEmpty5 { } // Noncompliant
    public class InnerEmpty6 { }             // Noncompliant

    public class InnerEmptyWithComments      // Noncompliant
    {
        // Some comment
    }

    class InnerNonEmpty
    {
        public int SomeProperty => 42;
    }
}

static class StaticEmpty { }                 // Noncompliant

partial class PartialEmpty { }               // Noncompliant
partial class PartialEmpty
{
    public int SomeProperty => 42;
}

interface IMarker { }                        // Compliant - this rule only deals with classes

struct EmptyStruct { }                       // Compliant - this rule only deals with classes

enum EmptyEnum { }                           // Compliant - this rule only deals with classes

class { }                                    // Error

