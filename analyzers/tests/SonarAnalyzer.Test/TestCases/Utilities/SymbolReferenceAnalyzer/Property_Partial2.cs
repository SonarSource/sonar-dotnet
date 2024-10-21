namespace SonarAnalyzer.Test.TestCases.Utilities.SymbolReferenceAnalyzer;

public partial class Property_Partial
{
    public partial int Property { get; set; } // Declaration

    public void Reference2()
    {
        Property = 1;
    }
}
