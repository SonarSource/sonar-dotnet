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
    public class BreakOutsideSwitch : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "BreakOutsideSwitch";
        internal const string Description = "'break' should not be used outside of 'switch'";
        internal const string MessageFormat = "Refactor the code in order to remove this break statement.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.BreakStatement); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            BreakStatementSyntax breakNode = (BreakStatementSyntax)node;
            if (!IsInSwitch(breakNode))
            {
                addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
            }
        }

        private static bool IsInSwitch(BreakStatementSyntax node)
        {
            SyntaxNode ancestor = node.FirstAncestorOrSelf<SyntaxNode>(e =>
                e.IsKind(SyntaxKind.SwitchStatement) ||
                e.IsKind(SyntaxKind.WhileStatement) ||
                e.IsKind(SyntaxKind.DoStatement) ||
                e.IsKind(SyntaxKind.ForStatement) ||
                e.IsKind(SyntaxKind.ForEachStatement));

            return ancestor != null && ancestor.IsKind(SyntaxKind.SwitchStatement);
        }
    }
}
