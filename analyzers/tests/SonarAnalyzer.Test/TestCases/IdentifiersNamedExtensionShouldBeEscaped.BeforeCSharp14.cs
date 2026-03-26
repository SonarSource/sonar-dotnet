// The structure of this file must mirror IdentifiersNamedExtensionShouldBeEscaped.Latest.cs.
// Both files must be kept in sync — add new test cases to both files.
// Annotations differ: in C# 14, all noncompliant cases are compiler errors instead of S8368 diagnostics.

namespace Noncompliant
{
    namespace ClassType { class extension { } } // Noncompliant

    namespace Members
    {
        class @extension
        {
            extension() { } // Noncompliant
        }

        unsafe class MyClass
        {
            extension M() { return null; }              // Noncompliant
            extension Property { get { return null; } } // Noncompliant
            extension field;                            // Noncompliant

            extension[] ArrayReturn() { return null; }   // Noncompliant
            extension[] arrayField;                      // Noncompliant

            extension[][] JaggedArrayReturn() { return null; }  // Noncompliant
            extension[][] jaggedArrayField;                     // Noncompliant
            extension[,] MultiDimArrayReturn() { return null; } // Noncompliant
            extension[,] multiDimArrayField;                    // Noncompliant

            extension* PointerReturn() { return null; } // Noncompliant
            extension* pointerField;                    // Noncompliant

            extension this[int i] { get { return null; } }                            // Noncompliant
            public static extension operator +(MyClass a, MyClass b) { return null; } // Noncompliant
        }
    }

    namespace Nullable
    {
        struct @extension { }

        class MyClass
        {
            extension M() { return new @extension(); }  // Noncompliant
            extension? N() { return null; }             // Noncompliant
        }
    }

}

namespace Compliant
{
    namespace ClassType { class @extension { }; class MyExtension { } }

    namespace Members
    {
        class @extension { @extension() { } ~extension() { } }

        unsafe class MyClass
        {
            void extension() { }
            @extension N() { return null; }
            @extension Escaped { get { return null; } }
            @extension escapedField;

            @extension[] EscapedArrayReturn() { return null; }
            @extension[] escapedArrayField;
            System.Collections.Generic.List<extension> ListReturn() { return null; }
            System.Collections.Generic.List<extension> listField;
            System.Collections.Generic.List<@extension> EscapedListReturn() { return null; }
            System.Collections.Generic.List<@extension> escapedListField;

            @extension* EscapedPointerReturn() { return null; }
            @extension* escapedPointerField;

            @extension this[int i] { get { return null; } }
            public static @extension operator +(MyClass a, MyClass b) { return null; }
            public static explicit operator extension(MyClass x) { return null; }

            void ParameterMethod(extension x) { }
            void LocalVarMethod() { extension x = null; }
        }
    }

    namespace Events
    {
        delegate void @extension();

        class MyClass
        {
            event extension MyEvent;
            event @extension EscapedEvent;
        }
    }

    namespace QualifiedNames
    {
        class MyClass
        {
            Noncompliant.Members.extension QualifiedField;
            global::Noncompliant.Members.extension GlobalQualifiedField;
            global::extension GlobalDirectField;
        }
    }
}

class @extension { }
