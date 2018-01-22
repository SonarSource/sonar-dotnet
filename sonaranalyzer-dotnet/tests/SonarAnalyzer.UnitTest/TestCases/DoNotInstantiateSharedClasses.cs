using System.ComponentModel.Composition;

namespace Tests.Diagnostics
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SharedClass {}

    [PartCreationPolicy(CreationPolicy.NonShared)]
    class NonSharedClass { }

    [PartCreationPolicy(CreationPolicy.Any)]
    class AnyClass { }

    class NoAttr { }

    class Program
    {
        public void Foo()
        {
            new SharedClass(); // Noncompliant {{Refactor this code so that it doesn't invoke the constructor of this class.}}
//          ^^^^^^^^^^^^^^^^^
            new NonSharedClass();
            new AnyClass();
            new NoAttr();
        }
    }
}
