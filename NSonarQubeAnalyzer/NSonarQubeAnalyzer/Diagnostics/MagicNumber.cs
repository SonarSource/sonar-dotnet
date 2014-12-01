using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Threading;
using System.Text.RegularExpressions;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MagicNumber : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "MagicNumber";
        internal const string Description = "Magic number should not be used";
        internal const string MessageFormat = "Extract this magic number into a constant, variable declaration or an enum.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.NumericLiteralExpression); } }

        public IImmutableSet<string> Exceptions;

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            LiteralExpressionSyntax literal = (LiteralExpressionSyntax)node;

            if (!node.IsPartOfStructuredTrivia() &&
                !node.Ancestors().Any(e =>
                  e.IsKind(SyntaxKind.VariableDeclarator) ||
                  e.IsKind(SyntaxKind.EnumDeclaration) ||
                  e.IsKind(SyntaxKind.Attribute)) &&
                !Exceptions.Contains(literal.Token.Text))
            {
                addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
            }
        }
    }
}
