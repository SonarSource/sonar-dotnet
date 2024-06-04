using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Compliant
{
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

    class GenericNotEmpty<T>
    {
        void Method(T arg) { }
    }

    class GenericNotEmptyWithConstraints<T>
        where T : class
    {
        void Method(T arg) { }
    }

    partial class PartialNotEmpty
    {
        int Prop => 42;
    }

    partial class PartialEmpty { }               // Compliant - Source Generators and some frameworks use empty partial classes as placeholders

    [ComVisible(true)]
    class ClassWithAttribute { }                 // Compliant - types with attributes are ignored

    [ComVisible(true), Obsolete]
    class ClassWithMultipleAttributes { }

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

    class IntegerList : List<int> { }             // Compliant - creates a more specific type without adding new members to it (similar to using the typedef keyword in C/C++)
    class StringLookup<T> : Dictionary<string, T> { }
    interface IIntegerSet<T> : ISet<int> { }

    class Conditional                            // Compliant - it's not empty when the given symbol is defined
    {
#if NOTDEFINED
    public override string ToString()
    {
        return "Debug Text";
    }
#endif
    }
}

namespace NonCompliant
{
    class Empty { }                              // Noncompliant {{Remove this empty class, write its code or make it an "interface".}}
    //    ^^^^^

    public class PublicEmpty { }                 // Noncompliant

    internal class InternalEmpty { }             // Noncompliant

    class EmptyWithComments                      // Noncompliant
    {
        // Some comment
    }

    static class StaticEmpty { }                 // Noncompliant

    abstract class AbstractEmpty { }             // Noncompliant

    class GenericEmpty<T> { }                    // Noncompliant
    //    ^^^^^^^^^^^^
    class GenericEmptyWithConstraints<T>         // Noncompliant
        where T : class { }
}

namespace Ignore
{
    class { }                                    // Error [CS1001]

    []                                           // Error [CS1001]
    class AttributeError { }

    interface IMarker { }                        // Compliant - this rule only deals with classes

    class ImplementsMarker : IMarker { }         // Compliant - implements a marker interface

    struct EmptyStruct { }                       // Compliant - this rule only deals with classes

    enum EmptyEnum { }                           // Compliant - this rule only deals with classes

    class SomeCommand { }                        // Compliant, ignored because of the suffix
    class SomeEvent { }                          // Compliant, ignored because of the suffix
    class SomeMessage { }                        // Compliant, ignored because of the suffix
    class Some_Command { }                       // Compliant, ignored because of the suffix
    class SomeQuery { }                          // Compliant, ignored because of the suffix, https://github.com/SonarSource/sonar-dotnet/issues/9241
    class Someevent { }                          // Noncompliant
    class SOMEMESSAGE { }                        // Noncompliant
    class SomeCommandHandler { }                 // Noncompliant
    class MessageHandler { }                     // Noncompliant
    class Command { }                            // Compliant, ignored because of the suffix
    class Event { }                              // Compliant, ignored because of the suffix
    class Message { }                            // Compliant, ignored because of the suffix
    class Query { }                              // Compliant, ignored because of the suffix


    class AssemblyDoc { }                        // Compliant, used for DefaultDocumentation tool
    class NamespaceDoc { }                       // compliant, used for DefaultDocumentation tool
}

namespace ConstructorAccessibility
{
    public class PublicClassWithPublicConstructor { public PublicClassWithPublicConstructor() { } }
    public class PublicClassWithInternalConstructor { internal PublicClassWithInternalConstructor() { } }
    public class PublicClassWithProtectedConstructor { protected PublicClassWithProtectedConstructor() { } }
    internal class InternalClassWithPublicConstructor { public InternalClassWithPublicConstructor() { } }
    internal class InternalClassWithInternalConstructor { internal InternalClassWithInternalConstructor() { } }
    internal class InternalClassWithProtectedConstructor { protected InternalClassWithProtectedConstructor() { } }

    public class ConstructorIsPublicAlready1 : PublicClassWithPublicConstructor { }             // Noncompliant
    public class WidensConstructorAccessibility1 : PublicClassWithInternalConstructor { }       // Compliant
    public class WidensConstructorAccessibility2 : PublicClassWithProtectedConstructor { }      // Compliant

    internal class ConstructorIsPublicAlready2 : PublicClassWithPublicConstructor { }           // Noncompliant
    internal class ConstructorIsInternalAlready1 : PublicClassWithInternalConstructor { }       // Noncompliant
    internal class WidensConstructorAccessibility3 : PublicClassWithProtectedConstructor { }    // Compliant
    internal class ConstructorIsInternalAlready2 : InternalClassWithPublicConstructor { }       // Noncompliant
    internal class ConstructorIsInternalAlready3 : InternalClassWithInternalConstructor { }     // Noncompliant
    internal class WidensConstructorAccessibility4 : InternalClassWithProtectedConstructor { }  // Compliant

    public class ClassWithNestedClasses
    {
        protected class ProtectedClassWithPublicConstructor { public ProtectedClassWithPublicConstructor() { } }
        protected class ProtectedClassWithInternalConstructor { internal ProtectedClassWithInternalConstructor() { } }
        protected class ProtectedClassWithProtectedConstructor { protected ProtectedClassWithProtectedConstructor() { } }
        private class PrivateClassWithPublicConstructor { public PrivateClassWithPublicConstructor() { } }
        private class PrivateClassWithInternalConstructor { internal PrivateClassWithInternalConstructor() { } }
        private class PrivateClassWithProtectedConstructor { protected PrivateClassWithProtectedConstructor() { } }

        protected class ConstructorIsPublicAlready3 : PublicClassWithPublicConstructor { }              // Noncompliant
        protected class WidensConstructorAccessibility5 : PublicClassWithInternalConstructor { }        // Compliant
        protected class ConstructorIsProtectedAlready1 : PublicClassWithProtectedConstructor { }        // Noncompliant
        protected class ConstructorIsProtectedAlready2 : ProtectedClassWithPublicConstructor { }        // Noncompliant
        protected class WidensConstructorAccessibility6 : ProtectedClassWithInternalConstructor { }     // Compliant
        protected class ConstructorIsProtectedAlready3 : ProtectedClassWithProtectedConstructor { }     // Noncompliant

        private class PrivateClass1 : PublicClassWithPublicConstructor { }                              // Noncompliant
        private class PrivateClass2 : PublicClassWithInternalConstructor { }                            // Noncompliant
        private class PrivateClass3 : PublicClassWithProtectedConstructor { }                           // Noncompliant
        private class PrivateClass4 : InternalClassWithPublicConstructor { }                            // Noncompliant
        private class PrivateClass5 : InternalClassWithInternalConstructor { }                          // Noncompliant
        private class PrivateClass6 : InternalClassWithProtectedConstructor { }                         // Noncompliant
        private class PrivateClass7 : ProtectedClassWithPublicConstructor { }                           // Noncompliant
        private class PrivateClass8 : ProtectedClassWithInternalConstructor { }                         // Noncompliant
        private class PrivateClass9 : ProtectedClassWithProtectedConstructor { }                        // Noncompliant
        private class PrivateClass10 : ProtectedClassWithPublicConstructor { }                          // Noncompliant
        private class PrivateClass11 : ProtectedClassWithInternalConstructor { }                        // Noncompliant
        private class PrivateClass12 : ProtectedClassWithProtectedConstructor { }                       // Noncompliant
    }
}
