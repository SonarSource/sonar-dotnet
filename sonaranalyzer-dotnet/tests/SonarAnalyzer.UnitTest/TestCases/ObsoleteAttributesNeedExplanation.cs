using System;

namespace Tests.Diagnostics
{
    [Obsolete] // Noncompliant {{Add an explanation.}}
//   ^^^^^^^^
    class Program
    {
        [Obsolete] // Noncompliant
        enum Enum { foo, bar }

        [Obsolete] // Noncompliant
        Program() { }

        [Obsolete] // Noncompliant
        void Method() { }

        [Obsolete] // Noncompliant
        int Property { get; set; }

        [Obsolete] // Noncompliant
        int Field;

        [Obsolete] // Noncompliant
        event EventHandler Event;

        [Obsolete] // Noncompliant
        delegate void Delegate();
    }

    [Obsolete] // Noncompliant
    interface IInterface
    {
        [Obsolete] // Noncompliant
        void Method();
    }


    [Obsolete] // Noncompliant
    struct ProgramStruct
    {
        [Obsolete] // Noncompliant
        void Method() { }
    }


    [Obsolete("explanation")]
    class Program_Explained
    {
        [Obsolete("explanation")]
        enum Enum { foo, bar }

        [Obsolete("explanation")]
        Program_Explained() { }

        [Obsolete("explanation")]
        void Method() { }

        [Obsolete("explanation")]
        string Property { get; set; }

        [Obsolete("explanation", true)]
        int Field;

        [Obsolete("explanation", false)]
        event EventHandler Event;

        [Obsolete("explanation")]
        delegate void Delegate();
    }

    [Obsolete("explanation")]
    interface IInterface_Explained
    {
        [Obsolete("explanation")]
        void Method();
    }


    [Obsolete("explanation")]
    struct ProgramStruct_Explained
    {
        [Obsolete("explanation")]
        void Method() { }
    }

    class Program_NoAttribs
    {
        [CLSCompliant(false)]
        enum Enum { foo, bar }

        Program_NoAttribs() { }

        void Method() { }

        int Property { get; set; }

        int Field;

        event EventHandler Event;

        delegate void Delegate();
    }
}
