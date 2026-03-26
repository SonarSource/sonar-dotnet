// The structure of this file must mirror IdentifiersNamedExtensionShouldBeEscaped.BeforeCSharp14.cs.
// Both files must be kept in sync — add new test cases to both files.
// Annotations differ: C# 14 emits compiler errors for all noncompliant cases instead of S8368 diagnostics.

namespace Noncompliant
{
    namespace ClassType { class extension { } } // Error [CS9306] Types and aliases cannot be named 'extension'.

    namespace Members
    {
        class @extension
        {
            extension() { } // Error [CS9283, CS1031]
        }

        unsafe class MyClass
        {
            extension M() => default;      // Error [CS9281, CS9283, CS1031, CS1513, CS1514, CS1519]
            extension Property { get; }    // Error [CS9281, CS9283, CS1003, CS1026, CS1031, CS1519, CS1519]
            extension field;               // Error [CS9281, CS9283, CS1003, CS1026, CS1031]

            extension[] ArrayReturn() => default;   // Error [CS9283, CS1003, CS1001, CS0246, CS1003, CS9285, CS8124, CS1003, CS1026]
            extension[] arrayField;                 // Error [CS9283, CS1003, CS1001, CS0246, CS1026]

            extension[][] JaggedArrayReturn() => default;   // Error [CS9283, CS1003, CS1001, CS1001, CS0246, CS1003, CS9285, CS8124, CS1003, CS1026]
            extension[][] jaggedArrayField;                 // Error [CS9283, CS1003, CS1001, CS1001, CS0246, CS1026]
            extension[,] MultiDimArrayReturn() => default;  // Error [CS9283, CS1003, CS1001, CS0246, CS1003, CS9285, CS8124, CS1003, CS1026]
            extension[,] multiDimArrayField;                // Error [CS9283, CS1003, CS1001, CS0246, CS1026]

            extension* PointerReturn() { return null; } // Error [CS9283, CS1003, CS1031, CS1103, CS1003, CS9285, CS8124, CS1026, CS1519]
            extension* pointerField;                    // Error [CS9283, CS1003, CS1031, CS1103, CS1026]

            extension this[int i] => default;                                    // Error [CS9283, CS0027, CS1003, CS0270, CS1031, CS1525, CS1003, CS1003, CS1026]
            public static extension operator +(MyClass a, MyClass b) => default; // Error [CS0106, CS0106, CS9283, CS1003, CS1003, CS1031, CS1003, CS9285, CS1003, CS1026]
        }
    }

    namespace Nullable
    {
        struct @extension { }

        class MyClass
        {
            extension M() { return new @extension(); }  // Error [CS9283, CS9281, CS1031, CS1519, CS0106, CS1520, CS9282]
            extension? N() { return null; }             // Error [CS9283, CS1003, CS1031, CS1003, CS9285, CS8124, CS1026, CS1519]
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
            Noncompliant.Members.extension QualifiedField;               // no annotation: rule doesn't fire (C# 14), no compiler error (qualified access)
            global::Noncompliant.Members.extension GlobalQualifiedField; // no annotation: rule doesn't fire (C# 14), no compiler error (qualified access)
            global::extension GlobalDirectField;                          // no annotation: rule doesn't fire (C# 14), no compiler error (qualified access)
        }
    }
}

class @extension { }
