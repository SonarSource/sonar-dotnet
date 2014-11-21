using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyMethod : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S1186";
        internal const string Description = "Methods should not be empty";
        internal const string MessageFormat = "Add a nested comment explaining why this method is empty, throw an NotSupportedException or complete the implementation.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.MethodDeclaration); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclaration = (MethodDeclarationSyntax)node;
            if (methodDeclaration.Body != null && !methodDeclaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.VirtualKeyword)) && IsEmpty(methodDeclaration.Body))
            {
                addDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation()));
            }
        }

        private static bool IsEmpty(BlockSyntax node)
        {
            return !node.Statements.Any() && !ContainsComment(node);
        }

        public static bool ContainsComment(BlockSyntax node)
        {
            return ContainsComment(node.OpenBraceToken.TrailingTrivia) || ContainsComment(node.CloseBraceToken.LeadingTrivia);
        }

        private static bool ContainsComment(SyntaxTriviaList trivias)
        {
            return trivias.Any(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        }
    }
}
