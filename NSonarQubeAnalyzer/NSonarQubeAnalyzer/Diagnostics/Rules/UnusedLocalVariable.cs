using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.Understandability)]
    [SqaleConstantRemediation("5min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    [Tags("unused")]
    public class UnusedLocalVariable : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1481";
        internal const string Description = "Unused local variables should be removed";
        internal const string MessageFormat = "Remove this unused \"{0}\" local variable.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;
        
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public Solution CurrentSolution { get; set; }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(c =>
            {
                var compilation = CurrentSolution.Projects.First().GetCompilationAsync().Result;
                var syntaxTree = compilation.SyntaxTrees.First();
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var declaration = (VariableDeclaratorSyntax) c.Node;

                var symbol = semanticModel.GetDeclaredSymbol(declaration);

                if (symbol == null || symbol.Kind != SymbolKind.Local)
                {
                    return;
                }

                var references = GetReferencesForSymbol(symbol).ToList();

                if (references.Any())
                {
                    return;
                }

                c.ReportDiagnostic(Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), symbol.Name));
            },
                SyntaxKind.VariableDeclarator);
        }

        private IEnumerable<SyntaxNode> GetReferencesForSymbol(ISymbol symbol)
        {
            var references = SymbolFinder.FindReferencesAsync(symbol, CurrentSolution).Result.ToList();
            foreach (var referencedSymbol in references)
            {
                foreach (var referenceLocation in referencedSymbol.Locations)
                {
                    var syntaxTree = referenceLocation.Location.SourceTree;
                    yield return syntaxTree.GetRoot().FindNode(referenceLocation.Location.SourceSpan);
                }
            }
        }
    }
}
