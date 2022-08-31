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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Literal
    {
        public static ProgramState Process(SymbolicContext context, ILiteralOperationWrapper literal) =>
            Process(context, literal.Type);

        public static ProgramState Process(SymbolicContext context, IDefaultValueOperationWrapper defaultValue) =>
            Process(context, defaultValue.Type);

        private static ProgramState Process(SymbolicContext context, ITypeSymbol type) =>
            (NullValue(context, type) ?? NotNullValue(context, type)) is { } value
                ? context.State.SetOperationValue(context.Operation, value)
                : context.State;

        private static SymbolicValue NullValue(SymbolicContext context, ITypeSymbol type) =>
            context.Operation.Instance.ConstantValue is { HasValue: true, Value: null }
            && (type ?? ConvertedType(context.Operation.Parent)) is { IsReferenceType: true }
                ? SymbolicValue.Null
                : null;

        private static SymbolicValue NotNullValue(SymbolicContext context, ITypeSymbol type) =>
            context.Operation.Instance.ConstantValue is { HasValue: true }
            && (type ?? ConvertedType(context.Operation.Parent)).Is(KnownType.System_String)
                ? SymbolicValue.NotNull
                : null;

        private static ITypeSymbol ConvertedType(IOperation operation) =>
            IConversionOperationWrapper.IsInstance(operation)
                ? IConversionOperationWrapper.FromOperation(operation).Type
                : null;
    }
}
