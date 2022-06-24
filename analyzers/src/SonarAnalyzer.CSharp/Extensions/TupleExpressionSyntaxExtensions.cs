/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    public static class TupleExpressionSyntaxExtensions
    {
        /// <summary>
        /// Gets all flattened arguments of a tuple. For a nested tuple like <c>(1, (2, 3))</c>, the arguments are flattened to <c>[1, 2, 3]</c>.
        /// </summary>
        /// <param name="tupleExpression">The tuple to flatten.</param>
        /// <returns>An <see cref="ImmutableArray"/> of the flattened tuple arguments.</returns>
        public static ImmutableArray<ArgumentSyntax> AllArguments(this TupleExpressionSyntaxWrapper tupleExpression)
        {
            var builder = ImmutableArray.CreateBuilder<ArgumentSyntax>(initialCapacity: tupleExpression.Arguments.Count);
            CollectTupleElements(builder, tupleExpression.Arguments);
            return builder.ToImmutableArray();

            static void CollectTupleElements(ImmutableArray<ArgumentSyntax>.Builder builder, SeparatedSyntaxList<ArgumentSyntax> arguments)
            {
                foreach (var argument in arguments)
                {
                    if (TupleExpressionSyntaxWrapper.IsInstance(argument.Expression))
                    {
                        CollectTupleElements(builder, ((TupleExpressionSyntaxWrapper)argument.Expression).Arguments);
                    }
                    else
                    {
                        builder.Add(argument);
                    }
                }
            }
        }
    }
}
