/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ConditionalStructureSameImplementation : ConditionalStructureSameImplementationBase
    {
        protected static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
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
                    var switchSection = (SwitchSectionSyntax) c.Node;
                    var precedingSection = switchSection
                        .GetPrecedingSections()
                        .FirstOrDefault(
                            preceding => EquivalenceChecker.AreEquivalent(switchSection.Statements, preceding.Statements));

                    if (precedingSection != null)
                    {
                        ReportSection(c, switchSection, precedingSection);
                    }
                },
                SyntaxKind.SwitchSection);
        }

        private static void CheckStatement(SyntaxNodeAnalysisContext context, StatementSyntax statementToCheck,
            IEnumerable<StatementSyntax> precedingStatements)
        {
            var precedingStatement = precedingStatements
                .FirstOrDefault(preceding => EquivalenceChecker.AreEquivalent(statementToCheck, preceding));

            if (precedingStatement != null)
            {
                ReportStatement(context, statementToCheck, precedingStatement);
            }
        }

        private static void ReportSection(SyntaxNodeAnalysisContext context, SwitchSectionSyntax switchSection, SwitchSectionSyntax precedingSection)
        {
            ReportSyntaxNode(context, switchSection, precedingSection, "case");
        }

        private static void ReportStatement(SyntaxNodeAnalysisContext context, StatementSyntax statement, StatementSyntax precedingStatement)
        {
            ReportSyntaxNode(context, statement, precedingStatement, "branch");
        }

        private static void ReportSyntaxNode(SyntaxNodeAnalysisContext context, SyntaxNode node, SyntaxNode precedingNode, string errorMessageDiscriminator)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                rule,
                node.GetLocation(),
                additionalLocations: new[] { precedingNode.GetLocation() },
                messageArgs: new object[] { precedingNode.GetLineNumberToReport(), errorMessageDiscriminator }));
        }
    }
}
