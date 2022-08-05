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
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Pattern
    {
        public static ProgramState Process(SymbolicContext context, IIsPatternOperationWrapper isPattern) =>
            context.State[isPattern.Value] is { } value
            && isPattern.Pattern.WrappedOperation.Kind == OperationKindEx.ConstantPattern
            && ConstantCheck.ConstraintFromValue(IConstantPatternOperationWrapper.FromOperation(isPattern.Pattern.WrappedOperation).Value.ConstantValue.Value) is BoolConstraint boolPattern
            && PatternBoolConstraint(value, boolPattern) is { } newConstraint
            ? context.SetOperationConstraint(newConstraint)
            : context.State;

        public static ProgramState Process(SymbolicContext context, IRecursivePatternOperationWrapper recursive)
        {
            if( recursive.DeclaredSymbol is null)
            {
                return context.State;
            }
            else
            {
                var state = new IOperationWrapperSonar(recursive.WrappedOperation).Parent.AsIsPattern() is { } parentIsPattern
                                && parentIsPattern.Value.TrackedSymbol() is { } sourceSymbol
                                ? context.State.SetSymbolValue(recursive.DeclaredSymbol, context.State[sourceSymbol])  // ToDo: MMF-2563 should define relation between tested and declared symbol
                                : context.State;
                return state.SetSymbolConstraint(recursive.DeclaredSymbol, ObjectConstraint.NotNull);
            }
        }

        private static BoolConstraint PatternBoolConstraint(SymbolicValue value, BoolConstraint pattern) =>
            value.HasConstraint<BoolConstraint>()
                ? BoolConstraint.From(value.HasConstraint(pattern))
                : null; // We cannot take conclusive decision
    }
}
