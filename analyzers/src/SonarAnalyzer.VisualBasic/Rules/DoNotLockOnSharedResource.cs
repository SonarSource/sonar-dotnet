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
    public sealed class DoNotLockOnSharedResource : DoNotLockOnSharedResourceBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var lockStatement = (SyncLockStatementSyntax)c.Node;

                    if (IsLockOnThis(lockStatement.Expression) ||
                        IsLockOnStringLiteral(lockStatement.Expression) ||
                        IsLockOnForbiddenKnownType(lockStatement.Expression, c.SemanticModel))
                    {
                        c.ReportIssue(rule, lockStatement.Expression);
                    }
                },
                SyntaxKind.SyncLockStatement);
        }

        private static bool IsLockOnThis(ExpressionSyntax expression) =>
            expression.IsKind(SyntaxKind.MeExpression);

        private static bool IsLockOnStringLiteral(ExpressionSyntax expression) =>
            expression.IsKind(SyntaxKind.StringLiteralExpression);
    }
}
