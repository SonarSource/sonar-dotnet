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
    public class FileLines : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "FileLoc";
        internal const string Description = "File should not have too many lines";
        internal const string MessageFormat = "This file has {1} lines, which is greater than {0} authorized. Split it into smaller files.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.CompilationUnit); } }

        public int Maximum;

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            int lines = node.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
            if (lines > Maximum)
            {
                addDiagnostic(Diagnostic.Create(Rule, Location.None, Maximum, lines));
            }
        }
    }
}
