/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
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
                    context.ReportIssue(rule, conditions[currentIndex], conditions[i].GetLineNumberToReport().ToString());
                    return;
                }
            }
        }
    }
}
