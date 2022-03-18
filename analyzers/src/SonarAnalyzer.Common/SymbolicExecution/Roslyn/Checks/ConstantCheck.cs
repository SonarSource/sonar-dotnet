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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    internal class ConstantCheck : SymbolicCheck
    {
        protected override ProgramState PreProcessSimple(SymbolicContext context) =>
            ConstraintFromValue(context.Operation.Instance.ConstantValue.Value) is { } constraint
                ? context.SetOperationConstraint(constraint)
                : context.State;

        public static SymbolicConstraint ConstraintFromType(ITypeSymbol type) =>
            ConstraintFromValue(DefaultValue(type));

        private static SymbolicConstraint ConstraintFromValue(object value) =>
            value switch
            {
                // Update DefaultValue when adding new types
                true => BoolConstraint.True,
                false => BoolConstraint.False,
                _ => null
            };

        private static object DefaultValue(ITypeSymbol type) =>
            type.Is(KnownType.System_Boolean) ? false : null;
    }
}
