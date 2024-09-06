/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ArgumentListSyntaxExtensions
{
    public static ExpressionSyntax Get(this ArgumentListSyntax argumentList, int index) =>
        argumentList != null && argumentList.Arguments.Count > index
            ? argumentList.Arguments[index].Expression.RemoveParentheses()
            : null;

    /// <summary>
    /// Returns argument expressions for given parameter.
    ///
    /// There can be zero, one or more results based on parameter type (Optional or ParamArray/params).
    /// </summary>
    public static ImmutableArray<SyntaxNode> ArgumentValuesForParameter(this ArgumentListSyntax argumentList, SemanticModel model, string parameterName) =>
        argumentList is not null && new CSharpMethodParameterLookup(argumentList, model).TryGetSyntax(parameterName, out var expressions)
            ? expressions
            : ImmutableArray<SyntaxNode>.Empty;
}
