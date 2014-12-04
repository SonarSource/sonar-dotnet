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

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssignmentInsideSubExpression : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "AssignmentInsideSubExpression";
        internal const string Description = "Assignment should not be used inside sub-expressions";
        internal const string MessageFormat = "Extract this assignment outside of the sub-expression.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxKind.SubtractAssignmentExpression,
                    SyntaxKind.MultiplyAssignmentExpression,
                    SyntaxKind.DivideAssignmentExpression,
                    SyntaxKind.ModuloAssignmentExpression,
                    SyntaxKind.AndAssignmentExpression,
                    SyntaxKind.ExclusiveOrAssignmentExpression,
                    SyntaxKind.OrAssignmentExpression,
                    SyntaxKind.LeftShiftAssignmentExpression,
                    SyntaxKind.RightShiftAssignmentExpression);
            }
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            if (IsInSubExpression(node))
            {
                addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
            }
        }

        private static bool IsInSubExpression(SyntaxNode node)
        {
            ExpressionSyntax expression = node.Parent.FirstAncestorOrSelf<ExpressionSyntax>(ancestor => ancestor is ExpressionSyntax);

            return expression is ExpressionSyntax &&
                !expression.IsKind(SyntaxKind.ParenthesizedLambdaExpression) &&
                !expression.IsKind(SyntaxKind.SimpleLambdaExpression) &&
                !expression.IsKind(SyntaxKind.AnonymousMethodExpression) &&
                !expression.IsKind(SyntaxKind.ObjectInitializerExpression);
        }
    }
}
