using System;

class Empty { }                              // Noncompliant {{Remove this empty class, or add members to it.}}
//    ^^^^^

public class PublicEmpty { }                 // Noncompliant
internal class InternalEmpty { }             // Noncompliant

class EmptyWithComments                      // Noncompliant
{
    // Some comment
}

class ClassWithProperty
{
    int SomeProperty => 42;
}
class ClassWithField
{
    int SomeField = 42;
}
class ClassWithMethod
{
    void Method() { }
}
class ClassWithIndexer
{
    int this[int index] => 42;
}
class ClassWithDelegate
{
    delegate void MethodDelegate();
}
class ClassWithEvent
{
    event EventHandler CustomEvent;
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

class GenericEmpty<T> { }                    // Noncompliant
//    ^^^^^^^^^^^^
class GenericEmptyWithConstraints<T>         // Noncompliant
    where T : class
{
}

class GenericNotEmpty<T>
{
    void Method(T arg) { }
}
class GenericNotEmptyWithConstraints<T>
    where T : class
{
    void Method(T arg) { }
}


static class StaticEmpty { }                 // Noncompliant

partial class PartialEmpty { }               // Compliant - Source Generators and some frameworks use empty partial classes as placeholders

partial class PartialNotEmpty
{
    int Prop => 42;
}

interface IMarker { }                        // Compliant - this rule only deals with classes

struct EmptyStruct { }                       // Compliant - this rule only deals with classes

enum EmptyEnum { }                           // Compliant - this rule only deals with classes

class { }                                    // Error

