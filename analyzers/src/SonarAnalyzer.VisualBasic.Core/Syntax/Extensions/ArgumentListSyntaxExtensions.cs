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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class ArgumentListSyntaxExtensions
{
    public static ExpressionSyntax Get(this ArgumentListSyntax argumentList, int index) =>
        argumentList is not null && argumentList.Arguments.Count > index
            ? argumentList.Arguments[index].GetExpression().RemoveParentheses()
            : null;

    /// <summary>
    /// Returns argument expressions for given parameter.
    ///
    /// There can be zero, one or more results based on parameter type (Optional or ParamArray/params).
    /// </summary>
    public static ImmutableArray<SyntaxNode> ArgumentValuesForParameter(this ArgumentListSyntax argumentList, SemanticModel model, string parameterName) =>
        argumentList is not null
            && new VisualBasicMethodParameterLookup(argumentList, model).TryGetSyntax(parameterName, out var expressions)
                ? expressions
                : ImmutableArray<SyntaxNode>.Empty;
}
