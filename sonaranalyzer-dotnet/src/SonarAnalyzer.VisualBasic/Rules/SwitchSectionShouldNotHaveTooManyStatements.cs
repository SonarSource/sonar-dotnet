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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class SwitchSectionShouldNotHaveTooManyStatements : SwitchSectionShouldNotHaveTooManyStatementsBase
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var caseBlock = (CaseBlockSyntax)c.Node;

                    if (caseBlock.IsMissing)
                    {
                        return;
                    }

                    var statementsCount = caseBlock.Statements.SelectMany(GetInnerStatements).Count();
                    if (statementsCount > Threshold)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, caseBlock.CaseStatement.GetLocation(),
                            "'Case' block", statementsCount, Threshold, "procedure"));
                    }
                },
                SyntaxKind.CaseBlock,
                SyntaxKind.CaseElseBlock);
        }

        private IEnumerable<StatementSyntax> GetInnerStatements(StatementSyntax statement)
        {
            return statement.DescendantNodesAndSelf()
                .OfType<StatementSyntax>()
                .Where(s => !IsExcludedFromCount(s));

            bool IsExcludedFromCount(StatementSyntax s)
            {
                switch (s.Kind())
                {
                    // Blocks are excluded because they contain statements (duplicating the interesting part)
                    case SyntaxKind.CatchBlock:
                    case SyntaxKind.DoLoopUntilBlock:
                    case SyntaxKind.DoLoopWhileBlock:
                    case SyntaxKind.DoUntilLoopBlock:
                    case SyntaxKind.DoWhileLoopBlock:
                    case SyntaxKind.ElseBlock:
                    case SyntaxKind.ElseIfBlock:
                    case SyntaxKind.FinallyBlock:
                    case SyntaxKind.ForBlock:
                    case SyntaxKind.ForEachBlock:
                    case SyntaxKind.MultiLineIfBlock:
                    case SyntaxKind.SelectBlock:
                    case SyntaxKind.SimpleDoLoopBlock:
                    case SyntaxKind.SyncLockBlock:
                    case SyntaxKind.TryBlock:
                    case SyntaxKind.UsingBlock:
                    case SyntaxKind.WhileBlock:
                    case SyntaxKind.WithBlock:

                    // Don't count End statements
                    case SyntaxKind.EndIfStatement:
                    case SyntaxKind.EndSelectStatement:
                    case SyntaxKind.EndSyncLockStatement:
                    case SyntaxKind.EndTryStatement:
                    case SyntaxKind.EndUsingStatement:
                    case SyntaxKind.EndWhileStatement:
                    case SyntaxKind.EndWithStatement:

                    // Don't count the Do from Do...While and Do...Until
                    case SyntaxKind.SimpleDoStatement:

                    // Don't count the Next from For...Next
                    case SyntaxKind.NextStatement:

                    // Don't count single line if statements
                    // We will count the next statement on the line anyway
                    // Example:
                    // If foo Then bar = 2
                    case SyntaxKind.SingleLineIfStatement:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
