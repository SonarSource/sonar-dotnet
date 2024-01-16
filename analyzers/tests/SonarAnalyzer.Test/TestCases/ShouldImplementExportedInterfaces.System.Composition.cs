using System.Composition;

public interface IContract { }

[Export(typeof(IContract))] // Noncompliant
class DoesNotImplement
{
}

[Export(typeof(IContract))] // Compliant
class Implements : IContract
{
}
