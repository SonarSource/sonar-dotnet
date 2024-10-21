namespace SonarAnalyzer.Test.TestCases.Utilities.SymbolReferenceAnalyzer;

public partial class Property_Partial
{
    public partial int Property { get => 1; set { } } // Implementation

    public void Reference1()
    {
        Property = 1;
    }
}
