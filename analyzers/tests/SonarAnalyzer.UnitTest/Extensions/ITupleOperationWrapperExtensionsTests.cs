using System.Text;
using FluentAssertions.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class ITupleOperationWrapperExtensionsTests
    {
        [DataTestMethod]
        // Tuple expression on the right side of assignment
        [DataRow("_ = (1, 2);", "1", "2")]
        [DataRow("_ = (x: 1, y: 2);", "1", "2")]
        [DataRow("_ = (1, M());", "1", "M()")]
        [DataRow("_ = (1, (2, 3));", "1", "2", "3")]
        [DataRow("_ = (1, (2, 3), 4, (5, (6, 7)));", "1", "2", "3", "4", "5", "6", "7")]
        // Tuple deconstruction on the left side of assignment
        [DataRow("(var a, (var b, var c)) = (1, (2, 3));", "var a", "var b", "var c")]
        [DataRow("(var a, var b) = (1, 2);", "var a", "var b")]
        [DataRow("(var a, _) = (1, 2);", "var a", "_")]
        [DataRow("int a; (a, var b) = (1, M());", "a", "var b")]
        // Tuple declaration expression
        [DataRow("var (a, b) = (1, 2);", "a", "b")]
        [DataRow("var (a, (b, c)) = (1, (2, 3));", "a", "b", "c")]
        [DataRow("var (a, (_, c)) = (1, (2, 3));", "a", "_", "c")]
        public void AllElements_ElementsOfFirstFoundTupleAreExtracted(string tuple, params string[] expectedElements)
        {
            var tupleOperation = CompileFirstTupleOperation(tuple);
            var allElements = tupleOperation.AllElements();
            allElements.Select(x => x.Syntax.ToString()).Should().BeEquivalentTo(expectedElements);
        }

        [TestMethod]
        public void AllElements_Performance_DeepNesting()
        {
            var deeplyNestedTuple = DeeplyNestedTuple(500);
            var tupleOperation = CompileFirstTupleOperation($"_ = {deeplyNestedTuple};");
            // Warm-up
            tupleOperation.AllElements();

            Action allElements = () => tupleOperation.AllElements();
            // Actual execution time is about 0.5ms.
            allElements.ExecutionTime().Should().BeLessThan(5.Milliseconds());

            static string DeeplyNestedTuple(int depth)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < depth; i++)
                {
                    sb.Append($"({i}, ");
                }
                sb.Append($"{depth}{new string(')', depth)}");
                return sb.ToString();
            }
        }

        [TestMethod]
        public void AllElements_Performance_LargeTuple()
        {
            var deeplyNestedTuple = LargeTuple(500);
            var tupleOperation = CompileFirstTupleOperation($"_ = {deeplyNestedTuple};");
            // Warm-up
            tupleOperation.AllElements();

            Action allElements = () => tupleOperation.AllElements();
            // Actual execution time is about 0.5ms.
            allElements.ExecutionTime().Should().BeLessThan(5.Milliseconds());

            static string LargeTuple(int length)
            {
                var sb = new StringBuilder();
                sb.Append("(");
                for (int i = 0; i < length; i++)
                {
                    sb.Append($"{i}, ");
                }
                sb.Append($"{length})");
                return sb.ToString();
            }
        }

        private static ITupleOperationWrapper CompileFirstTupleOperation(string tuple)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(tuple));
            var compilation = CSharpCompilation.Create("TempAssembly.dll")
                 .AddSyntaxTrees(syntaxTree)
                 .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                 .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            compilation.GetDiagnostics().Should().BeEmpty();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var tupleExpression = syntaxTree.GetRoot().DescendantNodes().First(x => x.IsAnyKind(SyntaxKind.TupleExpression, SyntaxKindEx.ParenthesizedVariableDesignation));
            return ITupleOperationWrapper.FromOperation(semanticModel.GetOperation(tupleExpression));
        }

        private static string WrapInMethod(string code) =>
$@"public class C
{{
    public int M()
    {{
        {code};
        return 0;
    }}
}}";
    }
}
