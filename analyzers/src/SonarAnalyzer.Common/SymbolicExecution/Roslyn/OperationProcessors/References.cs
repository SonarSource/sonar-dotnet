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

using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class References
    {
        public static ProgramState ProcessThis(SymbolicContext context) =>
            context.State.SetOperationValue(context.Operation, SymbolicValue.This);     // Implicit and Explicit

        public static ProgramState Process(SymbolicContext context, ILocalReferenceOperationWrapper localReference) =>
            context.State[localReference.Local] is { } value
                ? context.State.SetOperationValue(context.Operation, value)
                : context.State;

        public static ProgramState Process(SymbolicContext context, IParameterReferenceOperationWrapper parameterReference) =>
            context.State[parameterReference.Parameter] is { } value
                ? context.State.SetOperationValue(context.Operation, value)
                : context.State;

        public static ProgramState Process(SymbolicContext context, IFieldReferenceOperationWrapper fieldReference)
        {
            var state = context.State[fieldReference.Field] is { } value
                ? context.State.SetOperationValue(context.Operation, value)
                : context.State;
            return fieldReference.Instance.TrackedSymbol() is { } symbol
                ? state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : state;
        }

        public static ProgramState Process(SymbolicContext context, IPropertyReferenceOperationWrapper propertyReference) =>
            propertyReference.Instance.TrackedSymbol() is { } symbol
                ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : context.State;

        public static ProgramState Process(SymbolicContext context, IArrayElementReferenceOperationWrapper arrayElementReference) =>
            arrayElementReference.ArrayReference.TrackedSymbol() is { } symbol
                ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : context.State;

        public static ProgramState Process(SymbolicContext context, IEventReferenceOperationWrapper eventReference) =>
            eventReference.Instance.TrackedSymbol() is { } symbol
                ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : context.State;
    }
}
