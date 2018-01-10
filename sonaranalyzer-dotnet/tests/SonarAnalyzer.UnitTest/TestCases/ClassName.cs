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

    interface IIIFoo // Compliant
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

    struct ILMarker // Compliant
    {

    }

    [System.Runtime.InteropServices.ComImport()]
    internal interface SVsLog  // Compliant
    {
    }

    class IILMarker { } // Noncompliant {{Rename class 'IILMarker' to match camel case naming rules, consider using 'IilMarker'.}}

    interface IILMarker { } // Compliant

    interface ITVImageScraper { }

    class A4 { }
    class AA4 { }

    class AbcDEFgh { } // Compliant
    class Ab4DEFgh { } // Compliant
    class Ab4DEFGh { } // Noncompliant {{Rename class 'Ab4DEFGh' to match camel case naming rules, consider using 'Ab4DefGh'.}}

    class TTTestClassTTT { }// Noncompliant {{Rename class 'TTTestClassTTT' to match camel case naming rules, consider using 'TtTestClassTtt'.}}
    class TTT44 { }// Noncompliant
    class ABCDEFGHIJK { }// Noncompliant
    class Abcd4a { }// Noncompliant

    class A_B_C { } // Noncompliant;

    class AB { } // Compliant
    class AbABaa { } // Compliant
    class _AbABaa { } // Noncompliant {{Rename class '_AbABaa' to match camel case naming rules, trim underscores from the name.}}

    class 你好 { } // Compliant

    public partial class ELN { }
}
