using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSonarQubeAnalyzer.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ForLoopCounterCondition : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1994";
        internal const string Description = @"""for"" loop incrementers should modify the variable being tested in the loop's stop condition";
        internal const string MessageFormat = @"This loop's stop condition does not test variables used in the incrementer.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }
        
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    
                    var incrementorSymbols = GetIncrementorSymbols(forNode, c.SemanticModel).ToList();

                    //for find all reference
                    //var compilation = CurrentSolution.Projects.First().GetCompilationAsync().Result;
                    //var semanticModel = compilation.GetSemanticModel(c.Node.SyntaxTree);
                    //var incrementorSymbols = GetIncrementorSymbols(forNode, semanticModel).ToList();

                    if (!incrementorSymbols.Any())
                    {
                        return;
                    }

                    var conditionSymbols = GetReadSymbolsCondition(forNode, c.SemanticModel).ToList();

                    if (conditionSymbols.Intersect(incrementorSymbols).Any())
                    {
                        return;
                    }

                    c.ReportDiagnostic(Diagnostic.Create(Rule, forNode.GetLocation()));

                },
                SyntaxKind.ForStatement);
        }

        //private IEnumerable<SyntaxNode> GetReferencesForSymbol(ISymbol symbol)
        //{
        //    var references = SymbolFinder.FindReferencesAsync(symbol, CurrentSolution).Result.ToList();
        //    foreach (var referencedSymbol in references)
        //    {
        //        foreach (var referenceLocation in referencedSymbol.Locations)
        //        {
        //            var syntaxTree = referenceLocation.Location.SourceTree;
        //            yield return syntaxTree.GetRoot().FindNode(referenceLocation.Location.SourceSpan);
        //        }
        //    }
        //}

        private static IEnumerable<ISymbol> GetIncrementorSymbols(ForStatementSyntax forNode,
            SemanticModel semanticModel)
        {
            var accessedSymbols = new List<ISymbol>();
            foreach (var expressionSyntax in forNode.Incrementors)
            {
                var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(expressionSyntax);

                if (!dataFlowAnalysis.Succeeded)
                {
                    continue;
                }

                accessedSymbols.AddRange(dataFlowAnalysis.WrittenInside);
                accessedSymbols.AddRange(dataFlowAnalysis.ReadInside);
            }

            return accessedSymbols.Distinct();
        }

        private static IEnumerable<ISymbol> GetReadSymbolsCondition(ForStatementSyntax forNode,
            SemanticModel semanticModel)
        {
            if (forNode.Condition == null)
            {
                return new ISymbol[0];
            }

            var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(forNode.Condition);

            if (!dataFlowAnalysis.Succeeded)
            {
                return new ISymbol[0];
            }

            return dataFlowAnalysis.ReadInside.Distinct();
        }
    }
}
