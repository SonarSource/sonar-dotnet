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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    internal class ConstantCheck : SymbolicCheck
    {
        protected override ProgramState PreProcessSimple(SymbolicContext context) =>
            context.Operation.Instance.ConstantValue.HasValue
            && ConstraintFromConstantValue(context.Operation) is { } value
                ? context.State.SetOperationValue(context.Operation, value)
                : context.State;

        public static SymbolicConstraint ConstraintFromType(ITypeSymbol type)
        {
            if (type.Is(KnownType.System_Boolean))
            {
                return BoolConstraint.False;
            }
            else if (type.IsReferenceType)
            {
                return ObjectConstraint.Null;
            }
            else
            {
                return null;
            }
        }

        private static SymbolicValue ConstraintFromConstantValue(IOperationWrapperSonar operation) =>
            operation.Instance.ConstantValue.Value switch
            {
                // Update DefaultValue when adding new types
                true => SymbolicValue.True,
                false => SymbolicValue.False,
                null when (operation.Instance.Type ?? ConvertedType(operation.Parent)) is { IsReferenceType: true } => SymbolicValue.Null,
                string => SymbolicValue.NotNull,
                _ => null
            };

        private static ITypeSymbol ConvertedType(IOperation operation) =>
            operation.Kind == OperationKindEx.Conversion ? IConversionOperationWrapper.FromOperation(operation).Type : null;
    }
}
