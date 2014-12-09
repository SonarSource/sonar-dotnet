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
            context.RegisterCompilationEndAction(
                c =>
                {
                    var syntaxTree = c.Compilation.SyntaxTrees.Single();
                    var semanticModel = c.Compilation.GetSemanticModel(syntaxTree);

                    var variableDeclaratorNodes = syntaxTree
                        .GetCompilationUnitRoot()
                        .DescendantNodesAndSelf()
                        .Where(e => e.IsKind(SyntaxKind.VariableDeclarator))
                        .Select(e => (VariableDeclaratorSyntax)e);

                    var referencedSymbols = ReferencedSymbols(syntaxTree, semanticModel);

                    foreach (var variableDeclaratorNode in variableDeclaratorNodes)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(variableDeclaratorNode);

                        if (symbol.Kind == SymbolKind.Local && !referencedSymbols.Contains(symbol))
                        {
                            foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
                            {
                                c.ReportDiagnostic(Diagnostic.Create(Rule, syntaxReference.GetSyntax().GetLocation(), symbol.Name));
                            }
                        }
                    }
                });
        }

        // TODO Dirty workaround for Microsoft.CodeAnalysis.FindSymbols.SymbolFinder not working as expected
        private IImmutableSet<ISymbol> ReferencedSymbols(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var builder = ImmutableHashSet.CreateBuilder<ISymbol>();

            foreach (SyntaxNode node in syntaxTree.GetCompilationUnitRoot().DescendantNodesAndSelf())
            {
                var symbolInfo = semanticModel.GetSymbolInfo(node);
                if (symbolInfo.Symbol != null)
                {
                    builder.Add(symbolInfo.Symbol);
                }
                builder.UnionWith(symbolInfo.CandidateSymbols);
            }

            return builder.ToImmutable();
        }
    }
}
