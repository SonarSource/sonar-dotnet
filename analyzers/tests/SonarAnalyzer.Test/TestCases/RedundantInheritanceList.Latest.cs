namespace CSharp15
{
    interface IBase { }
    interface IDerived : IBase { }

    public class ClassRedundantInterface : IDerived, IBase { } // Noncompliant
    public union UnionRedundantInterface(UnionCaseA, UnionCaseB) : IDerived, IBase { } // FN NET-4959
    public union UnionNonRedundantInterface(UnionCaseA, UnionCaseB) : IDerived { } // Compliant
    public record class UnionCaseA(string Name);
    public record class UnionCaseB(string Name);
}
