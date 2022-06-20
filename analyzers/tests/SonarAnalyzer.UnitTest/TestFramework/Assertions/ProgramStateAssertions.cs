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

using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.TestFramework.Assertions;

public class ProgramStateAssertions : ReferenceTypeAssertions<ProgramState, ProgramStateAssertions>
{
    public ProgramStateAssertions(ProgramState instance) : base(instance)
    {
    }

    protected override string Identifier => "ProgramState";

    public AndConstraint<ProgramStateAssertions> HaveNoException()
    {
        Execute.Assertion.ForCondition(Subject.Exception == null).FailWith("Expected program state to have no exception but found {0}", Subject.Exception);
        return new AndConstraint<ProgramStateAssertions>(this);
    }

    public AndConstraint<ProgramStateAssertions> HaveUnknownException()
    {
        Execute.Assertion.ForCondition(Subject.Exception == ExceptionState.UnknownException).FailWith("Expected program state to have an unknown exception but found {0}", Subject.Exception);
        return new AndConstraint<ProgramStateAssertions>(this);
    }

    public AndConstraint<ProgramStateAssertions> HaveExceptionOfTypeException()
    {
        Execute.Assertion.ForCondition(Subject.Exception.ToString() == "Exception").FailWith("Expected program state to have an exception of type exception but fount {0}", Subject.Exception);
        return new AndConstraint<ProgramStateAssertions>(this);
    }
}
