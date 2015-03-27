using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Simplification;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace NSonarQubeAnalyzer.Diagnostics.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [SqaleSubCharacteristic(SqaleSubCharacteristic.LogicReliability)]
    [SqaleConstantRemediation("10min")]
    [Rule(DiagnosticId, RuleSeverity, Description, IsActivatedByDefault)]
    public class ConditionalStructureSameImplementation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1871";
        internal const string Description = @"Two branches in the same conditional structure should not have exactly the same implementation";
        internal const string MessageFormat = @"Either merge this {1} with the identical one on line {0} or change one of the implementations.";
        internal const string Category = "SonarQube";
        internal const Severity RuleSeverity = Severity.Major; 
        internal const bool IsActivatedByDefault = true;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, RuleSeverity.ToDiagnosticSeverity(), IsActivatedByDefault);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var ifStatement = (IfStatementSyntax)c.Node;

                    var precedingStatements = ifStatement
                        .GetPrecedingStatementsInConditionChain()
                        .ToList();

                    CheckStatement(c, ifStatement.Statement, precedingStatements);

                    if (ifStatement.Else == null) 
                    {
                        return;
                    }

                    precedingStatements.Add(ifStatement.Statement);
                    CheckStatement(c, ifStatement.Else.Statement, precedingStatements);                                       
                },
                SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var eqChecker = new EquivalenceChecker(c.SemanticModel);
                    var switchSection = (SwitchSectionSyntax)c.Node;
                    var switchStatements = eqChecker.GetExpandedList(switchSection.Statements);
                    var precedingSection = switchSection
                        .GetPrecedingSections()
                        .FirstOrDefault(preceding => eqChecker.AreEquivalent(switchStatements, preceding.Statements, false));

                    if (precedingSection != null) 
                    {
                        ReportSection(c, switchSection, precedingSection);
                    }                    
                },
                SyntaxKind.SwitchSection);
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext c, StatementSyntax statementToCheck, 
            IEnumerable<StatementSyntax> precedingStatements)
        {
            var expandedStatementToCheck = Simplifier.Expand(statementToCheck, c.SemanticModel, new AdhocWorkspace());

            var precedingStatement = precedingStatements
                .FirstOrDefault(preceding => new EquivalenceChecker(c.SemanticModel).AreEquivalent(expandedStatementToCheck, preceding, false));

            if (precedingStatement != null)
            {
                ReportStatement(c, statementToCheck, precedingStatement);
            }
        }

        private static void ReportSection(SyntaxNodeAnalysisContext c, SwitchSectionSyntax switchSection, SwitchSectionSyntax precedingSection)
        {
            ReportSyntaxNode(c, switchSection, precedingSection, "case");
        }

        private static void ReportStatement(SyntaxNodeAnalysisContext c, StatementSyntax statement, StatementSyntax precedingStatement)
        {
            ReportSyntaxNode(c, statement, precedingStatement, "branch");
        }

        private static void ReportSyntaxNode(SyntaxNodeAnalysisContext c, SyntaxNode node, SyntaxNode precedingNode, string errorMessageDiscriminator)
        {
            c.ReportDiagnostic(Diagnostic.Create(
                           Rule,
                           node.GetLocation(),
                           precedingNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1, 
                           errorMessageDiscriminator));
        } 
    }
}
