using System;

file class ClassWontBeExtended { } // Noncompliant {{Private or File classes or records which are not derived in the current assembly should be marked as 'sealed'.}}
//         ^^^^^^^^^^^^^^^^^^^

file class FileClassVirtualMethod // Compliant, the class has a virtual member.
{
    public virtual void AMethod() { }
}

file class FileClassVirtualProperty // Compliant, the class has a virtual member.
{
    public virtual int Number { get; set; }
}

file record FileClassVirtualIndexer // Compliant, the class has a virtual member.
{
    public virtual int this[int key]
    {
        get { return 1; }
        set { key += 1; }
    }
}

file record FileClassVirtualEvent // Compliant, the class has a virtual member.
{
    public virtual event EventHandler Foo
    {
        add
        {
            Console.WriteLine("Base Foo.add called");
        }
        remove
        {
            Console.WriteLine("Base Foo.remove called");
        }
    }
}


file class FileDerivedClass { } // Compliant, derived

file class FileDerivedClassExtension : FileDerivedClass { } // Noncompliant

file class FileDerivedClassSecondExtension : FileDerivedClass { }

file class TheThirdExtension : FileDerivedClassSecondExtension { } // Noncompliant

file sealed class ClassWontBeExtendedButFile { }

file struct AStruct { } // Compliant, structs cannot be inherited.

file static class FileStaticClass { } // Compliant, static classes cannot be inherited.

file abstract class FileAbstractClass { } // Compliant, abstract classes cannot be sealed.


namespace GenericClasses
{
    file class NotInheritedGenericClass<T> { } // Noncompliant
    file class InheritedGenericClass<T> { } // Compliant

    file class ImplementationClass : InheritedGenericClass<int> { } // Noncompliant
    file class ImplementationClass<T> : InheritedGenericClass<T> { } // Noncompliant

    file sealed class SealedImplementationClass : InheritedGenericClass<int> { } // Compliant
    file sealed class SealedImplementationClass<T> : InheritedGenericClass<T> { } // Compliant
    abstract class AbstractImplementationClass : InheritedGenericClass<int> { } // Compliant
    abstract class AbstractImplementationClass<T> : InheritedGenericClass<T> { } // Compliant 
}

namespace GenericRecords
{
    file record NotInheritedGenericRecord<T> { } // Noncompliant
    file record InheritedGenericRecord<T> { } // Compliant

    file record ImplementationRecord : InheritedGenericRecord<int> { } // Noncompliant
    file record ImplementationRecord<T> : InheritedGenericRecord<T> { } // Noncompliant

    file sealed record SealedImplementationRecord : InheritedGenericRecord<int> { } // Compliant
    file sealed record SealedImplementationRecord<T> : InheritedGenericRecord<T> { } // Compliant
    abstract record AbstractImplementationRecord : InheritedGenericRecord<int> { } // Compliant
    abstract record AbstractImplementationRecord<T> : InheritedGenericRecord<T> { } // Compliant
}

