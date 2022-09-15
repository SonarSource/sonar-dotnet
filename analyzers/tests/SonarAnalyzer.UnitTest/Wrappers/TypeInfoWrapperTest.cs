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
        public void NullabilityInfoFromShimEqualsOriginal()
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
            var identifier = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>().First(); // o in o.ToString()
            var typeInfo = semanticModel.GetTypeInfo(identifier);
            var convertedNullability = typeInfo.ConvertedNullability();
            convertedNullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
                .And.BeEquivalentTo(new
                {
                    typeInfo.ConvertedNullability.Annotation,
                    typeInfo.ConvertedNullability.FlowState
                }, options => options.ComparingEnumsByName());

            var nullability = typeInfo.Nullability();
            nullability.Should().Be(new NullabilityInfo(NullableAnnotation.Annotated, NullableFlowState.MaybeNull))
                .And.BeEquivalentTo(new
                {
                    typeInfo.Nullability.Annotation,
                    typeInfo.Nullability.FlowState
                }, options => options.ComparingEnumsByName());
        }
    }
}
