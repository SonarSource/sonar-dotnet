using System.ComponentModel.Composition;

class PrimaryConstructors
{
    [PartCreationPolicy(CreationPolicy.Any)] // Noncompliant
    class Class1(int iPar, in int iParWithExplicitIn, int iParWithDefault = 42);

    [InheritedExport(typeof(object))]
    [PartCreationPolicy(CreationPolicy.Any)] // Compliant, InheritedExport is present
    class Class2(int iPar, in int iParWithExplicitIn, int iParWithDefault = 42);
}
