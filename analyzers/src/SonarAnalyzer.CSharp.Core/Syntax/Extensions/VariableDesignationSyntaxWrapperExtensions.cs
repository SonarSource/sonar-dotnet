/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
