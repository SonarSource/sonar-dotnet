namespace SonarAnalyzer.Test.TestCases.Utilities.SymbolReferenceAnalyzer;

public partial class Method_Partial
{
    public partial void Implemented1();    // Implementation in Partial1
    public partial void Implemented2() { } // Implementation here

    public void Reference2()
    {
        Unimplemented();
        Implemented1();
        Implemented2();
    }
}
