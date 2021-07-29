using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

#if CS
using Microsoft.CodeAnalysis.CSharp.Syntax;
#else
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif

namespace SonarAnalyzer.Extensions
{
    internal static partial class MemberAccessExpressionSyntaxExtensions
    {
        public static bool IsPtrZero(this MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel) =>
                    memberAccess.Name.Identifier.Text == "Zero"
                    && semanticModel.GetTypeInfo(memberAccess).Type is var type
                    && type.IsAny(KnownType.System_IntPtr, KnownType.System_UIntPtr);
    }
}
