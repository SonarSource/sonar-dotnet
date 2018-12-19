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
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ConditionalStructureSameCondition : ConditionalStructureSameConditionBase
    {
        internal static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var ifBlock = (MultiLineIfBlockSyntax)c.Node;

                    var conditions = new[] { ifBlock.IfStatement?.Condition }
                        .Concat(ifBlock.ElseIfBlocks.Select(elseIf => elseIf.ElseIfStatement?.Condition))
                        .WhereNotNull()
                        .Select(cond => cond.RemoveParentheses())
                        .ToList();

                    for (var i = 1; i < conditions.Count; i++)
                    {
                        CheckConditionAt(i, conditions, c);
                    }
                },
                SyntaxKind.MultiLineIfBlock);
        }

        private static void CheckConditionAt(int currentIndex, List<ExpressionSyntax> conditions, SyntaxNodeAnalysisContext context)
        {
            for (var j = 0; j < currentIndex; j++)
            {
                if (VisualBasicEquivalenceChecker.AreEquivalent(conditions[currentIndex], conditions[j]))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, conditions[currentIndex].GetLocation(),
                        conditions[j].GetLineNumberToReport()));
                    return;
                }
            }
        }
    }
}
