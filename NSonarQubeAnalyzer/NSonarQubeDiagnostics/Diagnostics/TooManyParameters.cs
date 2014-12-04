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
    public class TooManyParameters : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        internal const string DiagnosticId = "S107";
        internal const string Description = "Functions should not have too many parameters";
        internal const string MessageFormat = "{2} has {1} parameters, which is greater than the {0} authorized.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest { get { return ImmutableArray.Create(SyntaxKind.ParameterList); } }

        public int Maximum;

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            var parameterList = (ParameterListSyntax)node;
            int parameters = parameterList.Parameters.Count;

            if (parameters > Maximum)
            {
                addDiagnostic(Diagnostic.Create(Rule, parameterList.GetLocation(), Maximum, parameters, ExtractName(node)));
            }
        }

        private static string ExtractName(SyntaxNode node)
        {
            string result;
            if (node.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                result = "Constructor \"" + ((ConstructorDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.MethodDeclaration))
            {
                result = "Method \"" + ((MethodDeclarationSyntax)node).Identifier + "\"";
            }
            else if (node.IsKind(SyntaxKind.DelegateDeclaration))
            {
                result = "Delegate";
            }
            else
            {
                result = "Lambda";
            }
            return result;
        }
    }
}
