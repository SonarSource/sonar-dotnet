using System;

class Empty { }                              // Noncompliant {{Remove this empty class, write its code or make it an "interface".}}
//    ^^^^^

public class PublicEmpty { }                 // Noncompliant
internal class InternalEmpty { }             // Noncompliant

class EmptyWithComments                      // Noncompliant
{
    // Some comment
}

class NotEmpty
{
    public int SomeProperty => 0;
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
        public int SomeProperty => 0;
    }
}

static class StaticEmpty { }                 // Noncompliant

partial class PartialEmpty { }               // Noncompliant
partial class PartialEmpty
{
    public int SomeProperty => 0;
}

interface IMarker { }                        // Compliant - using marker interfaces vs. attributes is a topic of debate, but they are commonly used

struct EmptyStruct { }                       // Compliant - this rule only deals with classes

enum EmptyEnum { }                           // Compliant - this rule only deals with classes

class { }                                    // Error


// Razor Pages: PageModel

