/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NonAsyncTaskShouldNotReturnNull : NonAsyncTaskShouldNotReturnNullBase
    {
        private const string MessageFormat = "Do not return null from this method, instead return 'Task.FromResult<T>(null)', " +
            "'Task.CompletedTask' or 'Task.Delay(0)'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> TrackedNullLiteralLocations =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.ArrowExpressionClause,
                SyntaxKind.ReturnStatement
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var nullLiteral = (LiteralExpressionSyntax)c.Node;

                    if (!nullLiteral.GetFirstNonParenthesizedParent().IsAnyKind(TrackedNullLiteralLocations))
                    {
                        return;
                    }

                    var enclosingMember = GetEnclosingMember(nullLiteral);
                    if (enclosingMember != null &&
                        !enclosingMember.IsKind(SyntaxKind.VariableDeclaration) &&
                        IsInvalidEnclosingSymbolContext(enclosingMember, c.Model))
                    {
                        c.ReportIssue(rule, nullLiteral);
                    }
                },
                SyntaxKind.NullLiteralExpression);
        }

        private static SyntaxNode GetEnclosingMember(LiteralExpressionSyntax literal)
        {
            foreach (var ancestor in literal.Ancestors())
            {
                switch (ancestor.Kind())
                {
                    case SyntaxKind.ParenthesizedLambdaExpression:
                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.VariableDeclaration:
                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKindEx.LocalFunctionStatement:
                        return ancestor;
                }
            }

            return null;
        }
    }
}
