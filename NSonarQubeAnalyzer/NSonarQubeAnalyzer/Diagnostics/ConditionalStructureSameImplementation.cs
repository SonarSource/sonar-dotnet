using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NSonarQubeAnalyzer.Diagnostics
{
    public class ConditionalStructureSameImplementation : DiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1871";
        internal const string Description = @"Two branches in the same conditional structure should not have exactly the same implementation";
        internal const string MessageFormat = @"Either merge this {1} with the identical one on line {0} or change one of the implementations.";
        internal const string Category = "SonarQube";
        internal const DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, Severity, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var ifStatement = (IfStatementSyntax)c.Node;

                    var preceedingStatements = ifStatement
                        .GetPreceedingStatementsInConditionChain()
                        .ToList();

                    CheckStatement(c, ifStatement.Statement, preceedingStatements);

                    if (ifStatement.Else == null) 
                    {
                        return;
                    }

                    preceedingStatements.Add(ifStatement.Statement);
                    CheckStatement(c, ifStatement.Else.Statement, preceedingStatements);                                       
                },
                SyntaxKind.IfStatement);

            context.RegisterSyntaxNodeAction(
                c =>
                {
                    var switchSection = (SwitchSectionSyntax)c.Node;
                    var preceedingSection = switchSection
                        .GetPreceedingSections()
                        .FirstOrDefault(preceeding => SyntaxFactory.AreEquivalent(switchSection.Statements, preceeding.Statements));

                    if (preceedingSection != null) 
                    {
                        ReportSection(c, switchSection, preceedingSection);
                    }                    
                },
                SyntaxKind.SwitchSection);
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext c, StatementSyntax statementToCheck, List<StatementSyntax> preceedingStatements)
        {
            var preceedingStatement = preceedingStatements
                .FirstOrDefault(preceeding => SyntaxFactory.AreEquivalent(statementToCheck, preceeding));

            if (preceedingStatement != null)
            {
                ReportStatement(c, statementToCheck, preceedingStatement);
            }
        }

        private static void ReportSection(SyntaxNodeAnalysisContext c, SwitchSectionSyntax switchSection, SwitchSectionSyntax preceedingSection)
        {
            ReportSyntaxNode(c, switchSection, preceedingSection, "case");
        }

        private static void ReportStatement(SyntaxNodeAnalysisContext c, StatementSyntax statement, StatementSyntax preceedingStatement)
        {
            ReportSyntaxNode(c, statement, preceedingStatement, "branch");
        }

        private static void ReportSyntaxNode(SyntaxNodeAnalysisContext c, SyntaxNode node, SyntaxNode preceedingNode, string errorMessageDiscriminator)
        {
            c.ReportDiagnostic(Diagnostic.Create(
                           Rule,
                           node.GetLocation(),
                           preceedingNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1, 
                           errorMessageDiscriminator));
        } 
    }
}
