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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class TupleExpressionSyntaxExtensions
{
    public static ImmutableArray<ArgumentSyntax> AllArguments(this TupleExpressionSyntaxWrapper tupleExpression)
    {
        var builder = ImmutableArray.CreateBuilder<ArgumentSyntax>(tupleExpression.Arguments.Count);
        CollectTupleElements(tupleExpression.Arguments);
        return builder.ToImmutableArray();

        void CollectTupleElements(SeparatedSyntaxList<ArgumentSyntax> arguments)
        {
            foreach (var argument in arguments)
            {
                if (TupleExpressionSyntaxWrapper.IsInstance(argument.Expression))
                {
                    CollectTupleElements(((TupleExpressionSyntaxWrapper)argument.Expression).Arguments);
                }
                else
                {
                    builder.Add(argument);
                }
            }
        }
    }
}
