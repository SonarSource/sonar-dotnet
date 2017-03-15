namespace Tests.Diagnostics
{
    class FSM // Noncompliant {{Rename class 'FSM' to match camel case naming rules, consider using 'Fsm'.}}
//        ^^^
    {
    }
    static class IEnumerableExtensions // Compliant
    {

    }

    class foo // Noncompliant {{Rename class 'foo' to match camel case naming rules, consider using 'Foo'.}}
    {
    }

    interface foo // Noncompliant {{Rename interface 'foo' to match camel case naming rules, consider using 'IFoo'.}}
    {
    }

    interface Foo // Noncompliant  {{Rename interface 'Foo' to match camel case naming rules, consider using 'IFoo'.}}
    {
    }

    interface IFoo
    {
    }

    interface IIFoo
    {
    }

    interface I
    {
    }

    interface II
    {
    }

    interface IIIFoo // Noncompliant {{Rename interface 'IIIFoo' to match camel case naming rules, consider using 'IIiFoo'.}}
    {
    }

    partial class Foo
    {
    }

    class MyClass
    {
        class I
        {
        }
    }

    class IFoo2 // Noncompliant {{Rename class 'IFoo2' to match camel case naming rules, consider using 'Foo2'.}}
    {
    }

    class Iden42TityFoo
    {
    }

    partial class
    Foo
    {
    }

    partial class
    AbClass_Bar // Noncompliant {{Rename class 'AbClass_Bar' to match camel case naming rules, consider using 'AbClassBar'.}}
    {
    }

    struct ILMarker // Noncompliant {{Rename struct 'ILMarker' to match camel case naming rules, consider using 'IlMarker'.}}
    {

    }

    [System.Runtime.InteropServices.ComImport()]
    internal interface SVsLog  // Compliant
    {
    }

    class A4 { }
    class AA4 { }

    class AbcDEFgh { } // Noncompliant {{Rename class 'AbcDEFgh' to match camel case naming rules, consider using 'AbcDeFgh'.}}
    class Ab4DEFgh { } // Noncompliant

    class TTTestClassTTT { }// Noncompliant {{Rename class 'TTTestClassTTT' to match camel case naming rules, consider using 'TtTestClassTtt'.}}
    class TTT44 { }// Noncompliant
    class ABCDEFGHIJK { }// Noncompliant
    class Abcd4a { }// Noncompliant

    class A_B_C { } // Noncompliant;

    class AB { } // Noncompliant, special case
    class AbABaa { }
    class _AbABaa { } // Noncompliant {{Rename class '_AbABaa' to match camel case naming rules, trim underscores from the name.}}
}
