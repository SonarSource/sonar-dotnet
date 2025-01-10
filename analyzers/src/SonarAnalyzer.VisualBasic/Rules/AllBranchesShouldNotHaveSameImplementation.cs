/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class AllBranchesShouldNotHaveSameImplementation : AllBranchesShouldNotHaveSameImplementationBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                new SelectCaseStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.SelectBlock);

            context.RegisterNodeAction(
                new TernaryStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.TernaryConditionalExpression);

            context.RegisterNodeAction(
                new IfStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.ElseBlock);

            context.RegisterNodeAction(
                new SingleLineIfStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.SingleLineElseClause);
        }

        private class IfStatementAnalyzer : IfStatementAnalyzerBase<ElseBlockSyntax, MultiLineIfBlockSyntax>
        {
            protected override bool IsLastElseInChain(ElseBlockSyntax elseSyntax) => true;

            protected override IEnumerable<SyntaxNode> GetStatements(ElseBlockSyntax elseSyntax) =>
                elseSyntax.Statements;

            protected override IEnumerable<IEnumerable<SyntaxNode>> GetIfBlocksStatements(ElseBlockSyntax elseSyntax,
                out MultiLineIfBlockSyntax topLevelIf)
            {
                topLevelIf = (MultiLineIfBlockSyntax)elseSyntax.Parent;
                return topLevelIf.ElseIfBlocks
                    .Select(elseif => elseif.Statements.Cast<SyntaxNode>())
                    .Concat(new[] { topLevelIf.Statements.Cast<SyntaxNode>() });
            }

            protected override Location GetLocation(MultiLineIfBlockSyntax topLevelIf)
                => topLevelIf.IfStatement.IfKeyword.GetLocation();
        }

        private class TernaryStatementAnalyzer : TernaryStatementAnalyzerBase<TernaryConditionalExpressionSyntax>
        {
            protected override SyntaxNode GetWhenFalse(TernaryConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenFalse.RemoveParentheses();

            protected override SyntaxNode GetWhenTrue(TernaryConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenTrue.RemoveParentheses();

            protected override Location GetLocation(TernaryConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.IfKeyword.GetLocation();
        }

        private class SingleLineIfStatementAnalyzer : IfStatementAnalyzerBase<SingleLineElseClauseSyntax, SingleLineIfStatementSyntax>
        {
            protected override IEnumerable<IEnumerable<SyntaxNode>> GetIfBlocksStatements(SingleLineElseClauseSyntax elseSyntax,
                out SingleLineIfStatementSyntax topLevelIf)
            {
                topLevelIf = (SingleLineIfStatementSyntax)elseSyntax.Parent;
                return new[] { topLevelIf.Statements.Cast<SyntaxNode>() };
            }

            protected override IEnumerable<SyntaxNode> GetStatements(SingleLineElseClauseSyntax elseSyntax) =>
                elseSyntax.Statements;

            protected override bool IsLastElseInChain(SingleLineElseClauseSyntax elseSyntax) => true;

            protected override Location GetLocation(SingleLineIfStatementSyntax topLevelIf) =>
                topLevelIf.IfKeyword.GetLocation();
        }

        private class SelectCaseStatementAnalyzer : SwitchStatementAnalyzerBase<SelectBlockSyntax, CaseBlockSyntax>
        {
            protected override bool AreEquivalent(CaseBlockSyntax section1, CaseBlockSyntax section2) =>
                SyntaxFactory.AreEquivalent(section1.Statements, section2.Statements);

            protected override IEnumerable<CaseBlockSyntax> GetSections(SelectBlockSyntax switchStatement) =>
                switchStatement.CaseBlocks;

            protected override bool HasDefaultLabel(SelectBlockSyntax switchStatement) =>
                switchStatement.CaseBlocks.Any(section => section.IsKind(SyntaxKind.CaseElseBlock));

            protected override Location GetLocation(SelectBlockSyntax switchStatement) =>
                switchStatement.SelectStatement.SelectKeyword.GetLocation();
        }
    }
}
