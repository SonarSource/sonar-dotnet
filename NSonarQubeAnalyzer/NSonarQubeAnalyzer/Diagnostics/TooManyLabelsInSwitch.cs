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
    public class TooManyLabelsInSwitch : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S1479";
        internal const string Description = "\"switch\" statements should not have too many \"case\" clauses";
        internal const string MessageFormat = "Reduce the number of switch cases from {1} to at most {0}.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.SwitchStatement); } }

        public int Maximum;

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            SwitchStatementSyntax switchStatement = (SwitchStatementSyntax)node;
            int labels = NumberOfLabels(switchStatement);

            if (labels > Maximum)
            {
                addDiagnostic(Diagnostic.Create(Rule, switchStatement.GetLocation(), Maximum, labels));
            }
        }

        private static int NumberOfLabels(SwitchStatementSyntax node)
        {
            return node.Sections.Sum(e => e.Labels.Count);
        }
    }
}
