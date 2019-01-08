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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class AllBranchesShouldNotHaveSameImplementationBase : SonarDiagnosticAnalyzer
    {
        protected const string SelectMessage =
            "Remove this '{0}' or edit its sections so that they are not all the same.";

        protected const string TernaryMessage =
            "Remove this ternary operator or edit it so that when true and when false blocks are not the same.";

        protected const string IfMessage =
            "Remove this '{0}' or edit its blocks so that they are not all the same.";

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

            public Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule, params object[] messageArgs) =>
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
                        var message = string.Format(IfMessage, messageArgs);
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, topLevelIf.GetLocation(), message));
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

            public Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule) =>
                context =>
                {
                    var ternaryStatement = (TTernaryStatement)context.Node;

                    var whenTrue = GetWhenTrue(ternaryStatement);
                    var whenFalse = GetWhenFalse(ternaryStatement);

                    if (whenTrue.IsEquivalentTo(whenFalse, topLevel: false))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(rule, ternaryStatement.GetLocation(), TernaryMessage));
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

            public Action<SyntaxNodeAnalysisContext> GetAnalysisAction(DiagnosticDescriptor rule, params object[] messageArgs) =>
                context =>
                {
                    var switchStatement = (TSwitchStatement)context.Node;

                    var sections = GetSections(switchStatement).ToList();

                    if (sections.Count >= 2 &&
                        HasDefaultLabel(switchStatement) &&
                        sections.Skip(1).All(section => AreEquivalent(section, sections[0])))
                    {
                        var message = string.Format(SelectMessage, messageArgs);
                        context.ReportDiagnostic(Diagnostic.Create(rule, switchStatement.GetLocation(), message));
                    }
                };
        }
    }
}
