using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class ITupleOperationWrapperExtensionsTests
    {
        [DataTestMethod]
        [DataRow("_ = (1, 2);", "1", "2")]
        [DataRow("_ = (x: 1, y: 2);", "1", "2")]
        [DataRow("_ = (1, M());", "1", "M()")]
        [DataRow("_ = (1, (2, 3));", "1", "2", "3")]
        [DataRow("_ = (1, (2, 3), 4, (5, (6, 7)));", "1", "2", "3", "4", "5", "6", "7")]

        [DataRow("(var a, (var b, var c)) = (1, (2, 3));", "var a", "var b", "var c")]
        [DataRow("(var a, var b) = (1, 2);", "var a", "var b")]
        [DataRow("(var a, _) = (1, 2);", "var a", "_")]
        [DataRow("int a; (a, var b) = (1, M());", "a", "var b")]

        [DataRow("var (a, b) = (1, 2);", "a", "b")]
        [DataRow("var (a, (b, c)) = (1, (2, 3));", "a", "b", "c")]
        [DataRow("var (a, (_, c)) = (1, (2, 3));", "a", "_", "c")]
        public void TupleElementsAreExtracted(string tuple, params string[] expectedElements)
        {
            var tupleOperation = CompileTupleOperation(tuple);
            var allElements = tupleOperation.AllElements();
            allElements.Select(x => x.Syntax.ToString()).Should().BeEquivalentTo(expectedElements);
        }

        private static ITupleOperationWrapper CompileTupleOperation(string tuple)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(tuple));
            var compilation = CSharpCompilation.Create("TempAssembly.dll")
                 .AddSyntaxTrees(syntaxTree)
                 .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                 .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.GetDiagnostics().Should().BeEmpty();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var tupleExpression = syntaxTree.GetRoot().DescendantNodesAndSelf().First(x => x.IsAnyKind(SyntaxKind.TupleExpression, SyntaxKindEx.ParenthesizedVariableDesignation));
            var tupleOperation = ITupleOperationWrapper.FromOperation(semanticModel.GetOperation(tupleExpression));
            return tupleOperation;
        }

        private static string WrapInMethod(string code) =>
$@"
public class C
{{
    public int M()
    {{
        {code};
        return 0;
    }}
}}
";
    }
}
