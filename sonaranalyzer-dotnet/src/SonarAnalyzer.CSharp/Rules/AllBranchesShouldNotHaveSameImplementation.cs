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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class AllBranchesShouldNotHaveSameImplementation : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3923";
        private const string MessageFormat = "{0}";
        private const string SwitchMessage = "Remove this switch or edit its sections so that they are not all the same.";
        private const string IfMessage = "Remove this if or edit its blocks so that they are not all the same.";
        private const string TernaryMessage = "Remove this ternary operator or edit it so that when true and when false blocks are not the same.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var switchStatement = (SwitchStatementSyntax)c.Node;

                    if (switchStatement.Sections.Count >= 2 &&
                        switchStatement.HasDefaultLabel() &&
                        switchStatement.Sections.Skip(1).All(section =>
                            SyntaxFactory.AreEquivalent(section.Statements, switchStatement.Sections[0].Statements, false)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, switchStatement.GetLocation(), SwitchMessage));
                    }
                },
                SyntaxKind.SwitchStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var elseClause = (ElseClauseSyntax)c.Node;
                    if (elseClause.Statement is IfStatementSyntax)
                    {
                        return;
                    }

                    var elseBlock = elseClause.Statement;

                    var allBlocks = new List<StatementSyntax>();
                    var currentIfStatement = elseClause.Parent as IfStatementSyntax;
                    var topLevelIfStatement = currentIfStatement;

                    while (currentIfStatement != null)
                    {
                        topLevelIfStatement = currentIfStatement;
                        allBlocks.Add(currentIfStatement.Statement);

                        // This trick avoids to keep walking up the tree when if is children of another if.
                        // For example, with the code below Parent.Parent would be IfStatementSyntax causing the code to produce
                        // wrong results: if (a) { if (b) { if (c) {} } } else { }
                        currentIfStatement = (currentIfStatement.Parent as ElseClauseSyntax)?.Parent as IfStatementSyntax;
                    }

                    if (topLevelIfStatement != null && // Should not happen but let's go defensive
                        allBlocks.All(block => block.IsEquivalentTo(elseBlock, false)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, topLevelIfStatement.GetLocation(), IfMessage));
                    }
                },
                SyntaxKind.ElseClause);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ternaryStatement = (ConditionalExpressionSyntax)c.Node;

                    if (ternaryStatement.WhenTrue.RemoveParentheses().IsEquivalentTo(
                            ternaryStatement.WhenFalse.RemoveParentheses(), false))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, ternaryStatement.GetLocation(), TernaryMessage));
                    }
                },
                SyntaxKind.ConditionalExpression);
        }
    }
}
