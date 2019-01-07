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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
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
                new SwitchStatementAnalyzer().GetAnalysisAction(rule, "switch"),
                SyntaxKind.SwitchStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new TernaryStatementAnalyzer().GetAnalysisAction(rule),
                SyntaxKind.ConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                new IfStatementAnalyzer().GetAnalysisAction(rule, "if"),
                SyntaxKind.ElseClause);
        }

        private class IfStatementAnalyzer : IfStatementAnalyzerBase<ElseClauseSyntax, IfStatementSyntax>
        {
            protected override bool IsLastElseInChain(ElseClauseSyntax elseSyntax) =>
                !(elseSyntax.Statement is IfStatementSyntax);

            protected override IEnumerable<SyntaxNode> GetStatements(ElseClauseSyntax elseSyntax) =>
                new[] { elseSyntax.Statement };

            protected override IEnumerable<IEnumerable<SyntaxNode>> GetIfBlocksStatements(ElseClauseSyntax elseSyntax,
                out IfStatementSyntax topLevelIf)
            {
                var allStatements = new List<IEnumerable<SyntaxNode>>();

                var currentElse = elseSyntax;

                topLevelIf = null;

                while (currentElse?.Parent is IfStatementSyntax currentIf)
                {
                    topLevelIf = currentIf;
                    allStatements.Add(new[] { currentIf.Statement });
                    currentElse = currentIf.Parent as ElseClauseSyntax;
                }

                return allStatements;
            }
        }

        private class TernaryStatementAnalyzer : TernaryStatementAnalyzerBase<ConditionalExpressionSyntax>
        {
            protected override SyntaxNode GetWhenFalse(ConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenFalse.RemoveParentheses();

            protected override SyntaxNode GetWhenTrue(ConditionalExpressionSyntax ternaryStatement) =>
                ternaryStatement.WhenTrue.RemoveParentheses();
        }

        private class SwitchStatementAnalyzer : SwitchStatementAnalyzerBase<SwitchStatementSyntax, SwitchSectionSyntax>
        {
            protected override bool AreEquivalent(SwitchSectionSyntax section1, SwitchSectionSyntax section2) =>
                SyntaxFactory.AreEquivalent(section1.Statements, section2.Statements);

            protected override IEnumerable<SwitchSectionSyntax> GetSections(SwitchStatementSyntax switchStatement) =>
                switchStatement.Sections;

            protected override bool HasDefaultLabel(SwitchStatementSyntax switchStatement) =>
                switchStatement.HasDefaultLabel();
        }
    }
}
