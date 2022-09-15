using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;
using NullabilityInfo = StyleCop.Analyzers.Lightup.NullabilityInfo;
using NullableAnnotation = StyleCop.Analyzers.Lightup.NullableAnnotation;
using NullableFlowState = StyleCop.Analyzers.Lightup.NullableFlowState;

namespace SonarAnalyzer.UnitTest.Wrappers
{
    [TestClass]
    public class TypeInfoWrapperTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var code = @"
#nullable enable
public class C
{
    public void M()
    {
        object? o = null;
        o.ToString();
    }
}
";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().First();
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            var convertedNullability = typeInfo.ConvertedNullability();
            convertedNullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull));
        }
    }
}
