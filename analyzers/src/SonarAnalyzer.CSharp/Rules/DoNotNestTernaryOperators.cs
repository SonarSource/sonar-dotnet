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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotNestTernaryOperators : DoNotNestTernaryOperatorsBase
    {
        private const string MessageFormat = "Extract this nested ternary operation into an independent statement.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    if (c.Node.Ancestors()
                        .TakeWhile(x => !(x?.Kind() is SyntaxKind.ParenthesizedLambdaExpression or SyntaxKind.SimpleLambdaExpression))
                        .OfType<ConditionalExpressionSyntax>()
                        .Any())
                    {
                        c.ReportIssue(Rule, c.Node);
                    }
                },
                SyntaxKind.ConditionalExpression);
    }
}
