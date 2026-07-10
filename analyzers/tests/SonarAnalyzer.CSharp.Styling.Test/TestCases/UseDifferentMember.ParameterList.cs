// Self-contained mimic of the real Microsoft.CodeAnalysis.CSharp / ShimLayer API shape, so this test does not depend on
// which Microsoft.CodeAnalysis.CSharp version is referenced (a native ParameterList property would otherwise shadow the
// extension property below and the bug this rule targets would never be reproduced).
namespace Fake
{
    public class ParameterListSyntax { }
    public class TypeDeclarationSyntax { }
    public class ClassDeclarationSyntax : TypeDeclarationSyntax { }
}

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions
{
    using Fake;

    public static class TypeDeclarationSyntaxExtensions
    {
        public static ParameterListSyntax ParameterList(this TypeDeclarationSyntax @this) => throw new System.NotImplementedException();
    }
}

namespace SonarAnalyzer.ShimLayer
{
    using Fake;

    public static class TypeDeclarationSyntaxShimExtensions
    {
        extension(TypeDeclarationSyntax @this)
        {
            public ParameterListSyntax ParameterList => throw new System.NotImplementedException();
        }
    }

    public static class ClassDeclarationSyntaxShimExtensions
    {
        extension(ClassDeclarationSyntax @this)
        {
            public ParameterListSyntax ParameterList => throw new System.NotImplementedException();
        }
    }

    public struct RecordDeclarationSyntaxWrapper
    {
        public ParameterListSyntax ParameterList => throw new System.NotImplementedException();
    }
}

// A file that only imports the ShimLayer namespace (the realistic vulnerable case: no SonarAnalyzer.CSharp.Core.Syntax.Extensions
// in scope, so bare 'ParameterList' access resolves silently to the shim property instead of failing with CS9339).
namespace TestBuggyUsage
{
    using Fake;
    using SonarAnalyzer.ShimLayer;

    internal class UseParameterList
    {
        public void Test(TypeDeclarationSyntax typeDeclaration, ClassDeclarationSyntax classDeclaration, RecordDeclarationSyntaxWrapper recordWrapper)
        {
            _ = typeDeclaration.ParameterList;  // Noncompliant {{Use 'ParameterList()' instead of 'ParameterList'. It resolves the parameter list correctly across all supported Roslyn versions.}}
            _ = classDeclaration.ParameterList; // Compliant, ClassDeclarationSyntax has its own dedicated shim property
            _ = recordWrapper.ParameterList;    // Compliant, RecordDeclarationSyntaxWrapper has its own dedicated property
        }
    }
}

namespace TestSafeUsage
{
    using Fake;
    using SonarAnalyzer.CSharp.Core.Syntax.Extensions;

    internal class UseParameterListSafe
    {
        public void Test(TypeDeclarationSyntax typeDeclaration) =>
            _ = typeDeclaration.ParameterList(); // Compliant, uses the extension method that dispatches correctly for every Roslyn version
    }
}
