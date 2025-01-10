/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public sealed class DoNotNestTernaryOperators : DoNotNestTernaryOperatorsBase
    {
        private const string MessageFormat = "Extract this nested If operator into independent If...Then...Else statements.";
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.Node.Ancestors()
                        .TakeWhile(x => !x.IsAnyKind(
                            SyntaxKind.MultiLineFunctionLambdaExpression,
                            SyntaxKind.SingleLineFunctionLambdaExpression,
                            SyntaxKind.MultiLineSubLambdaExpression,
                            SyntaxKind.SingleLineSubLambdaExpression))
                        .OfType<TernaryConditionalExpressionSyntax>()
                        .Any())
                    {
                        c.ReportIssue(Rule, c.Node);
                    }
                },
                SyntaxKind.TernaryConditionalExpression);
    }
}
