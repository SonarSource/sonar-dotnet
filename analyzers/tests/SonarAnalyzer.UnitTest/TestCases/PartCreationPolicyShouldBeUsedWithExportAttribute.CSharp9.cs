using System.ComponentModel.Composition;

[Export(typeof(object))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, Export is present
record Program1
{
}

[InheritedExport(typeof(object))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant, InheritedExport is present
record Program2_Base
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Compliant, InheritedExport is present in base
record Program2 : Program2_Base
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Noncompliant
record Program3
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Noncompliant
record Program4 : Program1
{
    public void Method()
    {
        [PartCreationPolicy(CreationPolicy.Any)] // Error [CS0592] - Compliant, attribute cannot be used on methods, don't raise
        void LocalFunction()
        { }
    }
}

[Export(typeof(Foo))]
[PartCreationPolicy(CreationPolicy.Any)] // Compliant
record Foo(string X)
{
}

[PartCreationPolicy(CreationPolicy.Any)] // Noncompliant
record Bar(string X)
{
}
