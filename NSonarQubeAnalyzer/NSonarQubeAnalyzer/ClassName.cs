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
    public class ClassName : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S[ID]";
        internal const string Description = "[Some description]";
        internal const string MessageFormat = "[Some message]";
        internal const string Category = "[Some category]";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
        {
            get
            {
                Console.WriteLine("Called for kinds!");
                return ImmutableArray.Create(
                    SyntaxKind.NamespaceDeclaration, SyntaxKind.MethodDeclaration,
                    SyntaxKind.NamespaceKeyword, SyntaxKind.MethodKeyword);
            }
        }

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                Console.WriteLine("Called for supported diagnostics");
                return ImmutableArray.Create(Rule);
            }
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            Console.WriteLine("Called on a node of type: " + node.RawKind);
            addDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}
