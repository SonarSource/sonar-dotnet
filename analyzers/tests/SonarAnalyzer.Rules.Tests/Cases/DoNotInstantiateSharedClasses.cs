using System.ComponentModel.Composition;

namespace Tests.Diagnostics
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SharedClass {}

    [System.ComponentModel.Composition.PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    class SharedClassFullNamespace { }

    [PartCreationPolicy(CreationPolicy.NonShared)]
    class NonSharedClass { }

    [PartCreationPolicy(CreationPolicy.Any)]
    class AnyClass { }

    [PartCreationPolicy(Foo)] // Error [CS0103] - Foo doesn't exist
    class InvalidAttrParameter { }

    [PartCreationPolicy()] // Error [CS7036]
    class NoAttrParameter { }

    class NoAttr { }

    class Program
    {
        public void Bar()
        {
            new SharedClass(); // Noncompliant {{Refactor this code so that it doesn't invoke the constructor of this class.}}
//          ^^^^^^^^^^^^^^^^^
            new SharedClassFullNamespace(); // Noncompliant
            new NonSharedClass();
            new AnyClass();
            new InvalidAttrParameter();
            new NoAttrParameter();
            new NoAttr();
        }
    }
}
