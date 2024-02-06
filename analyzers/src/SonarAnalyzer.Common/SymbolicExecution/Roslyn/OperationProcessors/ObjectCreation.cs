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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class ObjectCreation : SimpleProcessor<IObjectCreationOperationWrapper>
{
    protected override IObjectCreationOperationWrapper Convert(IOperation operation) =>
        IObjectCreationOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IObjectCreationOperationWrapper operation)
    {
        if (operation.Type.IsAny(EmptyCollectionsShouldNotBeEnumeratedBase.TrackedCollectionTypes)
            && CollectionCreationConstraint(context.State, operation) is { } constraint)
        {
            return context.SetOperationConstraint(constraint)
                .SetOperationConstraint(operation, ObjectConstraint.NotNull);
        }
        else if (operation.Type.IsNullableValueType())
        {
            if (operation.Arguments.IsEmpty)
            {
                return context.SetOperationConstraint(ObjectConstraint.Null);
            }
            else if (context.State[operation.Arguments.First().ToArgument().Value] is { } value)
            {
                return context.SetOperationValue(value.WithConstraint(ObjectConstraint.NotNull));
            }
            else
            {
                return context.SetOperationConstraint(ObjectConstraint.NotNull);
            }
        }
        else
        {
            return context.SetOperationConstraint(ObjectConstraint.NotNull);
        }
    }

    private static CollectionConstraint CollectionCreationConstraint(ProgramState state, IObjectCreationOperationWrapper operation) =>
        operation.Arguments.SingleOrDefault(IsEnumerable) is { } argument
            ? state.Constraint<CollectionConstraint>(argument)
            : CollectionConstraint.Empty;

    private static bool IsEnumerable(IOperation operation) =>
            operation.ToArgument().Parameter.Type.DerivesOrImplements(KnownType.System_Collections_IEnumerable);
}
