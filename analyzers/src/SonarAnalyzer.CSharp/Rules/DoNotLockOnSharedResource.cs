/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotLockOnSharedResource : DoNotLockOnSharedResourceBase
    {
        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var lockStatement = (LockStatementSyntax)c.Node;

                    if (IsLockOnThis(lockStatement.Expression) ||
                        IsLockOnStringLiteral(lockStatement.Expression) ||
                        IsLockOnForbiddenKnownType(lockStatement.Expression, c.Model))
                    {
                        c.ReportIssue(rule, lockStatement.Expression);
                    }
                },
                SyntaxKind.LockStatement);
        }

        private static bool IsLockOnThis(ExpressionSyntax expression) =>
            expression.IsKind(SyntaxKind.ThisExpression);

        private static bool IsLockOnStringLiteral(ExpressionSyntax expression) =>
            expression.IsKind(SyntaxKind.StringLiteralExpression);
    }
}
