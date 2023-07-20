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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ConditionalStructureSameCondition : ConditionalStructureSameConditionBase
    {
        protected override ILanguageFacade Language => VisualBasicFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var ifBlock = (MultiLineIfBlockSyntax)c.Node;
                    var conditions = new[] { ifBlock.IfStatement?.Condition }
                        .Concat(ifBlock.ElseIfBlocks.Select(x => x.ElseIfStatement?.Condition))
                        .WhereNotNull()
                        .Select(x => x.RemoveParentheses())
                        .ToArray();

                    for (var i = 1; i < conditions.Length; i++)
                    {
                        CheckConditionAt(c, conditions, i);
                    }
                },
                SyntaxKind.MultiLineIfBlock);

        private void CheckConditionAt(SonarSyntaxNodeReportingContext context, ExpressionSyntax[] conditions, int currentIndex)
        {
            for (var i = 0; i < currentIndex; i++)
            {
                if (VisualBasicEquivalenceChecker.AreEquivalent(conditions[currentIndex], conditions[i]))
                {
                    context.ReportIssue(CreateDiagnostic(rule, conditions[currentIndex].GetLocation(), conditions[i].GetLineNumberToReport()));
                    return;
                }
            }
        }
    }
}
