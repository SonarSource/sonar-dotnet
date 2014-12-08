using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Diagnostics;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Threading;

namespace NSonarQubeAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedLocalVariable : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1481";
        internal const string Description = "Unused local variables should be removed";
        internal const string MessageFormat = "Remove this unused \"{0}\" local variable.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var variableDeclaratorNode = (VariableDeclaratorSyntax)c.Node;
                    var symbol = c.SemanticModel.GetDeclaredSymbol(variableDeclaratorNode);

                    if (symbol.Kind == SymbolKind.Local && !HasReferences(symbol, c.Node.SyntaxTree, c.SemanticModel))
                    {
                        foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
                        {
                            c.ReportDiagnostic(Diagnostic.Create(Rule, syntaxReference.GetSyntax().GetLocation(), symbol.Name));
                        }
                    }
                },
                SyntaxKind.VariableDeclarator);
        }

        // TODO Dirty workaround for Microsoft.CodeAnalysis.FindSymbols.SymbolFinder not working as expected
        private bool HasReferences(ISymbol symbol, SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            foreach (SyntaxNode node in syntaxTree.GetCompilationUnitRoot().DescendantNodesAndSelf())
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                if (symbol.Equals(symbolInfo.Symbol) || symbolInfo.CandidateSymbols.Contains(symbol))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
