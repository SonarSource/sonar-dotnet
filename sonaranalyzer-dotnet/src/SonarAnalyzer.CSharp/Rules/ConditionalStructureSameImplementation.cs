/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> ignoredStatementsInSwitch = new HashSet<SyntaxKind>
        {
            SyntaxKind.BreakStatement,
            SyntaxKind.ReturnStatement,
            SyntaxKind.ThrowStatement,
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
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

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchSection = (SwitchSectionSyntax)c.Node;

                    if (GetStatements(switchSection).Count(IsApprovedStatement) < 2)
                    {
                        return;
                    }

                    var precedingSection = switchSection
                        .GetPrecedingSections()
                        .FirstOrDefault(
                            preceding => CSharpEquivalenceChecker.AreEquivalent(switchSection.Statements, preceding.Statements));

                    if (precedingSection != null)
                    {
                        ReportSyntaxNode(c, switchSection, precedingSection, "case");
                    }
                },
                SyntaxKind.SwitchSection);
        }

        private static IEnumerable<StatementSyntax> GetStatements(SwitchSectionSyntax switchSection)
        {
            return Enumerable.Empty<StatementSyntax>()
                .Union(switchSection.Statements.OfType<BlockSyntax>().SelectMany(block => block.Statements))
                .Union(switchSection.Statements.Where(s => !s.IsKind(SyntaxKind.Block)));
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement,
            IEnumerable<StatementSyntax> precedingStatements)
        {
            if (statement.ChildNodes().Count() < 2)
            {
                return;
            }

            var precedingStatement = precedingStatements
                .FirstOrDefault(preceding => CSharpEquivalenceChecker.AreEquivalent(statement, preceding));

            if (precedingStatement != null)
            {
                ReportSyntaxNode(context, statement, precedingStatement, "branch");
            }
        }

        private static void ReportSyntaxNode(SyntaxNodeAnalysisContext context, SyntaxNode node, SyntaxNode precedingNode, string errorMessageDiscriminator)
        {
            context.ReportDiagnosticWhenActive(Diagnostic.Create(
                rule,
                node.GetLocation(),
                additionalLocations: new[] { precedingNode.GetLocation() },
                messageArgs: new object[] { precedingNode.GetLineNumberToReport(), errorMessageDiscriminator }));
        }

        private static bool IsApprovedStatement(StatementSyntax statement)
        {
            return !statement.IsAnyKind(ignoredStatementsInSwitch);
        }
    }
}
