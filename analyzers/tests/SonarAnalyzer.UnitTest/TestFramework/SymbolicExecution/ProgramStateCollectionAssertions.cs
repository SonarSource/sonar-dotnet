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

using FluentAssertions.Collections;
using FluentAssertions.Execution;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

public static class ProgramStateCollectionAssertions
{
    public static AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState> ContainSingleNoException(this GenericCollectionAssertions<ProgramState> context) =>
        ContainsSingleException(context, state => state.Exception == null, "Expected given collection to contain a single state without exception ");

    public static AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState> ContainSingleUnknownException(this GenericCollectionAssertions<ProgramState> context) =>
        ContainsSingleException(context, state => state.Exception == ExceptionState.UnknownException, "Expected given collection to contain a single state with unknown exception ");

    public static AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState> ContainSingleSystemException(this GenericCollectionAssertions<ProgramState> context) =>
        ContainSingleExceptionOfType(context, "Exception");

    public static AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState> ContainSingleExceptionOfType(this GenericCollectionAssertions<ProgramState> context, string typeName) =>
        ContainsSingleException(context, state => state.Exception?.Type?.Name == typeName, $@"Expected given collection to contain a single state with exception of type ""{typeName}"" ");

    private static AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState> ContainsSingleException(GenericCollectionAssertions<ProgramState> context, Func<ProgramState, bool> predicate, string errorMessage)
    {
        Execute.Assertion.ForCondition(context.Subject.Any()).FailWith(errorMessage + "but the collection is empty.", Array.Empty<object>());

        var array = context.Subject.Where(predicate).ToArray();
        if (array.Length == 0)
        {
            Execute.Assertion.FailWith($"{errorMessage}but no such state was found.");
        }
        else if (array.Length > 1)
        {
            Execute.Assertion.FailWith($"{errorMessage}but {array.Length} such states were found.");
        }

        return new AndWhichConstraint<GenericCollectionAssertions<ProgramState>, ProgramState>(context, array);
    }
}
