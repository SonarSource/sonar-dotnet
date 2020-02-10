using Microsoft.CodeAnalysis;
using System.Linq;

namespace SonarAnalyzer.Helpers
{
    public static class SemanticModelExtensions
    {
        public static SyntaxNode GetDeclaringSyntaxNode(this SemanticModel semanticModel, SyntaxNode expressionSyntax) =>
            semanticModel
                .GetSymbolInfo(expressionSyntax)
                .Symbol?
                .DeclaringSyntaxReferences
                .FirstOrDefault()?
                .GetSyntax();
    }
}
