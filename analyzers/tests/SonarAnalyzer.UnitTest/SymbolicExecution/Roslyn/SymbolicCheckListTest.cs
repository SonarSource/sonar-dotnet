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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class SymbolicCheckListTest
    {
        [TestMethod]
        public void Constructor_Null_Throws() =>
            ((Func<SymbolicCheckList>)(() => new SymbolicCheckList(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("checks");

        [TestMethod]
        public void Notifications_ExecutedForAll()
        {
            var a = new FlagTestCheck();
            var b = new FlagTestCheck();
            var context = new SymbolicContext(new(), null, ProgramState.Empty);
            var sut = new SymbolicCheckList(new[] { a, b });

            a.WasExitReached.Should().BeFalse();
            b.WasExitReached.Should().BeFalse();
            sut.ExitReached(context);
            a.WasExitReached.Should().BeTrue();
            b.WasExitReached.Should().BeTrue();

            a.WasExecutionCompleted.Should().BeFalse();
            b.WasExecutionCompleted.Should().BeFalse();
            sut.ExecutionCompleted();
            a.WasExecutionCompleted.Should().BeTrue();
            b.WasExecutionCompleted.Should().BeTrue();

            a.WasPreProcessed.Should().BeFalse();
            b.WasPreProcessed.Should().BeFalse();
            sut.PreProcess(context);
            a.WasPreProcessed.Should().BeTrue();
            b.WasPreProcessed.Should().BeTrue();

            a.WasPostProcessed.Should().BeFalse();
            b.WasPostProcessed.Should().BeFalse();
            sut.PostProcess(context);
            a.WasPostProcessed.Should().BeTrue();
            b.WasPostProcessed.Should().BeTrue();
        }

        [TestMethod]
        public void PostProcess_CanReturnMultipleStates()
        {
            var triple = new PostProcessTestCheck(x => new[] { x.State, x.State, x.State });
            var sut = new SymbolicCheckList(new[] { triple, triple });
            sut.PostProcess(new(new(), null, ProgramState.Empty)).Should().HaveCount(9);
        }

        [TestMethod]
        public void PostProcess_CanReturnNoStates()
        {
            var empty = new PostProcessTestCheck(x => Array.Empty<ProgramState>());
            var sut = new SymbolicCheckList(new[] { empty });
            sut.PostProcess(new(new(), null, ProgramState.Empty)).Should().HaveCount(0);
        }

        private class FlagTestCheck : SymbolicCheck
        {
            public bool WasExitReached { get; private set; }
            public bool WasExecutionCompleted { get; private set; }
            public bool WasPreProcessed { get; private set; }
            public bool WasPostProcessed { get; private set; }

            public override void ExitReached(SymbolicContext context) =>
                WasExitReached = true;

            public override void ExecutionCompleted() =>
                WasExecutionCompleted = true;

            public override ProgramState[] PreProcess(SymbolicContext context)
            {
                WasPreProcessed = true;
                return base.PreProcess(context);
            }

            public override ProgramState[] PostProcess(SymbolicContext context)
            {
                WasPostProcessed = true;
                return base.PostProcess(context);
            }
        }
    }
}
