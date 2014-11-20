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
    public class TabCharacter : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "TabCharacter";
        internal const string Description = "Tabulation character should not be used";
        internal const string MessageFormat = "Replace all tab characters in this file by sequences of white-spaces.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.CompilationUnit); } }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            int offset = node.ToFullString().IndexOf('\t');
            if (offset >= 0)
            {
                var location = node.SyntaxTree.GetLocation(TextSpan.FromBounds(offset, offset));
                addDiagnostic(Diagnostic.Create(Rule, location));
            }
        }
    }
}
