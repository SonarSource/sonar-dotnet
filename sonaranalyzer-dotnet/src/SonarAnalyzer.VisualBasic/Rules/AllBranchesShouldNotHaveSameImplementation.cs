/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class AllBranchesShouldNotHaveSameImplementation : AllBranchesShouldNotHaveSameImplementationBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                new SelectCaseStatementAnalyzer().GetAnalysisAction(rule, "Select Case"),
                SyntaxKind.SelectBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new TernaryStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.TernaryConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new IfStatementAnalyzer().GetAnalysisAction(rule, "If"),
                SyntaxKind.ElseBlock);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new SingleLineIfStatementAnalyzer().GetAnalysisAction(rule, "If"),
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
        }

        private class TernaryStatementAnalyzer : TernaryStatementAnalyzerBase<TernaryConditionalExpressionSyntax>
        {
            protected override SyntaxNode GetWhenFalse(TernaryConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenFalse.RemoveParentheses();

            protected override SyntaxNode GetWhenTrue(TernaryConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenTrue.RemoveParentheses();
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
        }

        private class SelectCaseStatementAnalyzer : SwitchStatementAnalyzerBase<SelectBlockSyntax, CaseBlockSyntax>
        {
            protected override bool AreEquivalent(CaseBlockSyntax section1, CaseBlockSyntax section2) =>
                SyntaxFactory.AreEquivalent(section1.Statements, section2.Statements);

            protected override IEnumerable<CaseBlockSyntax> GetSections(SelectBlockSyntax switchStatement) =>
                switchStatement.CaseBlocks;

            protected override bool HasDefaultLabel(SelectBlockSyntax switchStatement) =>
                switchStatement.CaseBlocks.Any(section => section.IsKind(SyntaxKind.CaseElseBlock));
        }
    }
}
