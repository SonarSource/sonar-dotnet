using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class BlockSyntaxExtensionsTests
    {
        [TestMethod]
        public void IsEmpty_EmptyBlockMethod()
        {
            var block = @"
{

}
";
            var declaration = MethodDeclarationSyntax(WrapInClass(block));
            declaration.Body.IsEmpty().Should().BeTrue();
        }

        private static string WrapInClass(string methodBlockOrArrow)
        {
            return $@"
public class C
{{
    public void M()
    {methodBlockOrArrow}
}}
";
        }

        private static MethodDeclarationSyntax MethodDeclarationSyntax(string source)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();
            root.ContainsDiagnostics.Should().BeFalse();
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        }
    }
}
