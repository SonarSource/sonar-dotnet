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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    public static class VariableDesignationSyntaxWrapperExtensions
    {
        /// <summary>
        /// Returns all <see cref="SingleVariableDesignationSyntaxWrapper"/> of the designation. Nested designations are flattened and
        /// only identifiers are included in the result (discards are skipped). For a designation like <c>(a, (_, b))</c>
        /// the method returns <c>[a, b]</c>.
        /// </summary>
        /// <param name="variableDesignation">The designation to return the variables for.</param>
        /// <returns>A list of <see cref="SingleVariableDesignationSyntaxWrapper"/> that contain all flattened variables of the designation.</returns>
        public static ImmutableArray<SingleVariableDesignationSyntaxWrapper> AllVariables(this VariableDesignationSyntaxWrapper variableDesignation)
        {
            var builder = ImmutableArray.CreateBuilder<SingleVariableDesignationSyntaxWrapper>();
            CollectVariables(builder, variableDesignation);
            return builder.ToImmutableArray();

            static void CollectVariables(ImmutableArray<SingleVariableDesignationSyntaxWrapper>.Builder builder, VariableDesignationSyntaxWrapper variableDesignation)
            {
                if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(variableDesignation))
                {
                    var parenthesized = (ParenthesizedVariableDesignationSyntaxWrapper)variableDesignation;
                    foreach (var variable in parenthesized.Variables)
                    {
                        CollectVariables(builder, variable);
                    }
                }
                else if (SingleVariableDesignationSyntaxWrapper.IsInstance(variableDesignation))
                {
                    builder.Add((SingleVariableDesignationSyntaxWrapper)variableDesignation);
                }
                // DiscardDesignationSyntaxWrapper is skipped
            }
        }
    }
}
