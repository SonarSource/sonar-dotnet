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

public static class VariableDesignationSyntaxWrapperExtensions
{
    /// <summary>
    /// Returns all <see cref="SingleVariableDesignationSyntaxWrapper"/> of the designation. Nested designations are flattened and
    /// only identifiers are included in the result (discards are skipped). For a designation like <c>(a, (_, b))</c>
    /// the method returns <c>[a, b]</c>.
    /// </summary>
    public static ImmutableArray<SingleVariableDesignationSyntaxWrapper> AllVariables(this VariableDesignationSyntaxWrapper variableDesignation)
    {
        var builder = ImmutableArray.CreateBuilder<SingleVariableDesignationSyntaxWrapper>();
        CollectVariables(variableDesignation);
        return builder.ToImmutableArray();

        void CollectVariables(VariableDesignationSyntaxWrapper variableDesignation)
        {
            if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(variableDesignation))
            {
                foreach (var variable in ((ParenthesizedVariableDesignationSyntaxWrapper)variableDesignation).Variables)
                {
                    CollectVariables(variable);
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
