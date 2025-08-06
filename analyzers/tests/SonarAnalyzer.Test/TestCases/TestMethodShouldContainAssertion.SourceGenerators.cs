using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;


[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> calculatorClassesProvider = default;

        context.RegisterSourceOutput(calculatorClassesProvider, (sourceProductionContext, calculatorClass) => Execute());
    }

    public static void Execute() { }
}

[TestClass]
public class TestGeneratorUnitTest
{
    [TestMethod]
    public async Task Test() => // Noncompliant, FP - next line contains the assertion
        await new CSharpSourceGeneratorTest<SourceGenerator, DefaultVerifier>().RunAsync();
}
