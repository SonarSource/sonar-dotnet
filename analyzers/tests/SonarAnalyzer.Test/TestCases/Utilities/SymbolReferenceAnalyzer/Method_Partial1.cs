namespace SonarAnalyzer.Test.TestCases.Utilities.SymbolReferenceAnalyzer;

public partial class Method_Partial
{
    partial void Unimplemented();   // Declaration without implementation
    public partial void Implemented1() { } // Implementation here
    public partial void Implemented2();    // Implementation in Partial2

    partial void DeclaredAndImplemented();
    partial void DeclaredAndImplemented() { }

    public void Reference1()
    {
        Unimplemented();
        Implemented1();
        Implemented2();

        DeclaredAndImplemented();
    }
}
