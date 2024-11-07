namespace SonarAnalyzer.Test.TestCases.Utilities.SymbolReferenceAnalyzer;

public partial class Property_Partial
{
    public partial int Property { get => 1; set { } } // Implementation

    partial int DeclaredAndImplemented { get; }
    partial int DeclaredAndImplemented => 1;

    public void Reference1()
    {
        Property = 1;
        _ = DeclaredAndImplemented;
    }
}
