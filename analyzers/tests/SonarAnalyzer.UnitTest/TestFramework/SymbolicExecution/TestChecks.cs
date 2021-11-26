/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using StyleCop.Analyzers.Lightup;
using ProcessFunc = System.Func<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState, StyleCop.Analyzers.Lightup.IOperationWrapperSonar, SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;

namespace SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution
{
    internal class CollectorTestCheck : SymbolicExecutionCheck
    {
        // ToDo: Simplified version for now, we'll need ProgramState & Operation. Or even better, the whole exploded Node
        private readonly List<IOperationWrapperSonar> preProcessedOperations = new();

        public override ProgramState PreProcess(ProgramState state, IOperationWrapperSonar operation)
        {
            preProcessedOperations.Add(operation);
            return state;
        }

        public void ValidateOrder(params string[] expected) =>
            preProcessedOperations.Where(x => !x.IsImplicit).Select(TestHelper.Serialize).Should().OnlyContainInOrder(expected);
    }
}
