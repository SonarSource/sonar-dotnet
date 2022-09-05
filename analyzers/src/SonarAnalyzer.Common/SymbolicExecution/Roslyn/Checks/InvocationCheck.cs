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

using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    internal class InvocationCheck : SymbolicCheck
    {
        public override ProgramState[] PreProcess(SymbolicContext context) =>
            context.Operation.Instance.Kind == OperationKindEx.Invocation
            && PreProcess(context, IInvocationOperationWrapper.FromOperation(context.Operation.Instance)) is { } newState
                ? newState
                : base.PreProcess(context);

        private static ProgramState[] PreProcess(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            invocation.TargetMethod.IsAny(KnownType.System_String, nameof(string.IsNullOrEmpty), nameof(string.IsNullOrWhiteSpace))
                ? ProcessStringIsNullOrEmpty(context, invocation)
                : null;

        private static ProgramState[] ProcessStringIsNullOrEmpty(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            invocation.Arguments[0].TrackedSymbol() is { } argumentSymbol
                ? context.State[argumentSymbol]?.Constraint<ObjectConstraint>() switch
                {
                    ObjectConstraint constraint when constraint == ObjectConstraint.NotNull => null, // The "normal" state handling reflects already what is going on.
                    ObjectConstraint constraint when constraint == ObjectConstraint.Null =>
                        context.SetOperationConstraint(BoolConstraint.True), // Method will return "True" if argument is known to be Null
                    _ => new[] // Explode the known states, these methods can create.
                    {
                            context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.Null),
                            context.SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                            context.SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(argumentSymbol, ObjectConstraint.NotNull),
                    },
                }
                : null;
    }
}
