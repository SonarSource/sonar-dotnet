/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class AllBranchesShouldNotHaveSameImplementationBase : SonarDiagnosticAnalyzer
    {
        protected const string StatementsMessage =
            "Remove this conditional structure or edit its code blocks so that they're not all the same.";

        protected const string TernaryMessage =
            "This conditional operation returns the same value whether the condition is \"true\" or \"false\".";

        internal const string DiagnosticId = "S3923";
        internal const string MessageFormat = "{0}";

        protected abstract class IfStatementAnalyzerBase<TElseSyntax, TIfSyntax>
            where TElseSyntax : SyntaxNode
            where TIfSyntax : SyntaxNode
        {
            protected abstract IEnumerable<SyntaxNode> GetStatements(TElseSyntax elseSyntax);

            protected abstract IEnumerable<IEnumerable<SyntaxNode>> GetIfBlocksStatements(TElseSyntax elseSyntax,
                out TIfSyntax topLevelIf);

            protected abstract bool IsLastElseInChain(TElseSyntax elseSyntax);

            protected abstract Location GetLocation(TIfSyntax topLevelIf);

            public Action<SonarSyntaxNodeReportingContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
                context =>
                {
                    var elseSyntax = (TElseSyntax)context.Node;

                    if (!IsLastElseInChain(elseSyntax))
                    {
                        return;
                    }

                    var ifBlocksStatements = GetIfBlocksStatements(elseSyntax, out var topLevelIf);

                    var elseStatements = GetStatements(elseSyntax);

                    if (ifBlocksStatements.All(ifStatements => AreEquivalent(ifStatements, elseStatements)))
                    {
                        context.ReportIssue(CreateDiagnostic(rule, GetLocation(topLevelIf), StatementsMessage));
                    }
                };

            private static bool AreEquivalent(IEnumerable<SyntaxNode> nodes1, IEnumerable<SyntaxNode> nodes2) =>
                nodes1.Equals(nodes2, (x, y) => x.IsEquivalentTo(y, topLevel: false));
        }

        protected abstract class TernaryStatementAnalyzerBase<TTernaryStatement>
            where TTernaryStatement : SyntaxNode
        {
            protected abstract SyntaxNode GetWhenTrue(TTernaryStatement ternaryStatement);

            protected abstract SyntaxNode GetWhenFalse(TTernaryStatement ternaryStatement);

            protected abstract Location GetLocation(TTernaryStatement ternaryStatement);

            public Action<SonarSyntaxNodeReportingContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
                context =>
                {
                    var ternaryStatement = (TTernaryStatement)context.Node;

                    var whenTrue = GetWhenTrue(ternaryStatement);
                    var whenFalse = GetWhenFalse(ternaryStatement);

                    if (whenTrue.IsEquivalentTo(whenFalse, topLevel: false))
                    {
                        context.ReportIssue(CreateDiagnostic(rule, GetLocation(ternaryStatement), TernaryMessage));
                    }
                };
        }

        protected abstract class SwitchStatementAnalyzerBase<TSwitchStatement, TSwitchSection>
            where TSwitchStatement : SyntaxNode
            where TSwitchSection : SyntaxNode
        {
            protected abstract IEnumerable<TSwitchSection> GetSections(TSwitchStatement switchStatement);

            protected abstract bool HasDefaultLabel(TSwitchStatement switchStatement);

            protected abstract bool AreEquivalent(TSwitchSection section1, TSwitchSection section2);

            protected abstract Location GetLocation(TSwitchStatement switchStatement);

            public Action<SonarSyntaxNodeReportingContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
                context =>
                {
                    var switchStatement = (TSwitchStatement)context.Node;

                    var sections = GetSections(switchStatement).ToList();

                    if (sections.Count >= 2 &&
                        HasDefaultLabel(switchStatement) &&
                        sections.Skip(1).All(section => AreEquivalent(section, sections[0])))
                    {
                        context.ReportIssue(CreateDiagnostic(rule, GetLocation(switchStatement), StatementsMessage));
                    }
                };
        }
    }
}
