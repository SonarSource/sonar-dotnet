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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DateTimeFormatShouldNotBeHardcoded : DateTimeFormatShouldNotBeHardcodedBase<SyntaxKind, InvocationExpressionSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override Location HardCodedArgumentLocation(InvocationExpressionSyntax invocation) =>
        invocation.ArgumentList.Arguments[0].Expression.GetLocation();

    protected override bool HasInvalidFirstArgument(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
        invocation.ArgumentList is { }
        && invocation.ArgumentList.Arguments.Any()
        && GetFormatArgumentExpression(invocation.ArgumentList) is { } argumentExpression
        && argumentExpression.FindConstantValue(semanticModel) is string { Length: > 1 };

    private static ExpressionSyntax GetFormatArgumentExpression(ArgumentListSyntax argumentList) =>
        (argumentList.GetArgumentByName("format") ?? argumentList.Arguments[0]).Expression;
}
