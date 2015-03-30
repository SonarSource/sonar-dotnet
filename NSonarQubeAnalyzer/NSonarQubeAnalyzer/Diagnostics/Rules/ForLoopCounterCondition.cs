using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("20min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class ForLoopCounterCondition : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1994";
        internal const string Description = @"""for"" loop incrementers should modify the variable being tested in the loop's stop condition";
        internal const string MessageFormatNotEmpty = @"This loop's stop condition tests ""{0}"" but the incrementer updates ""{1}"".";
        internal const string MessageFormatEmpty = @"This loop's stop incrementer updates ""{0}"" but the stop condition doesn't test any variables.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Critical; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, "{0}", Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var forNode = (ForStatementSyntax)c.Node;
                    
                    var incrementedSymbols = GetIncrementorSymbols(forNode, c.SemanticModel).ToList();
                    
                    if (!incrementedSymbols.Any())
                    {
                        return;
                    }

                    var conditionSymbols = GetReadSymbolsCondition(forNode, c.SemanticModel).ToList();

                    if (conditionSymbols.Intersect(incrementedSymbols).Any())
                    {
                        return;
                    }

                    var incrementedVariables = string.Join(",", incrementedSymbols.Select(s => s.Name));
                    if (conditionSymbols.Any())
                    {
                        var conditionVariables = string.Join(",", conditionSymbols.Select(s=>s.Name));
                        c.ReportDiagnostic(Diagnostic.Create(Rule, forNode.GetLocation(),
                            string.Format(CultureInfo.InvariantCulture, MessageFormatNotEmpty, incrementedVariables, conditionVariables)));
                    }
                    else
                    {
                        c.ReportDiagnostic(Diagnostic.Create(Rule, forNode.GetLocation(),
                            string.Format(CultureInfo.InvariantCulture, MessageFormatEmpty, incrementedVariables)));
                    }

                },
                SyntaxKind.ForStatement);
        }

        private static IEnumerable<ISymbol> GetIncrementorSymbols(ForStatementSyntax forNode,
            SemanticModel semanticModel)
        {
            var accessedSymbols = new List<ISymbol>();
            foreach (var dataFlowAnalysis in forNode.Incrementors
                .Select(semanticModel.AnalyzeDataFlow)
                .Where(dataFlowAnalysis => dataFlowAnalysis.Succeeded))
            {
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

            return dataFlowAnalysis.Succeeded 
                ? dataFlowAnalysis.ReadInside.Distinct() 
                : new ISymbol[0];
        }
    }
}
