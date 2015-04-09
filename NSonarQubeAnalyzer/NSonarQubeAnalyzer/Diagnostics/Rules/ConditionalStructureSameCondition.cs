using System.Collections.Immutable;
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
    [SqaleConstantRemediation("10min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class ConditionalStructureSameCondition : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1862";
        internal const string Description = @"Related ""if/else if"" statements should not have the same condition";
        internal const string MessageFormat = @"This branch duplicates the one on line {0}.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Critical;
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var ifStatement = (IfStatementSyntax)c.Node;
                    var currentCondition = ifStatement.Condition;
                    
                    using (var eqChecker = new EquivalenceChecker(c.SemanticModel))
                    {
                        var precedingCondition = ifStatement
                            .GetPrecedingConditionsInConditionChain()
                            .FirstOrDefault(
                                preceding => eqChecker.AreEquivalent(currentCondition, preceding));

                        if (precedingCondition != null)
                        {
                            c.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                currentCondition.GetLocation(),
                                precedingCondition.GetLocation().GetLineSpan().StartLinePosition.Line + 1));
                        }
                    }
                },
                SyntaxKind.IfStatement);
        }        
    }
}
