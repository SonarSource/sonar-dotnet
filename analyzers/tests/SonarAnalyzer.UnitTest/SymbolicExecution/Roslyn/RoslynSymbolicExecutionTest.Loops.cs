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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [DataTestMethod]
        [DataRow("for (var i = 0; i < items.Length; i++)")]
        [DataRow("foreach (var i in items)")]
        [DataRow("while (value > 0)")]
        [DataRow("while (true)")]
        public void Loops_InstructionVisitedMaxTwice(string loop)
        {
            var code = $@"
var value = 42;
{loop}
{{
    value.ToString(); // Add another constraint to 'value'
    value--;
}}
Tag(""End"", value);";
            var validator = SETestContext.CreateCS(code, ", int[] items", new AddConstraintOnInvocationCheck()).Validator;
            validator.ValidateExitReachCount(2);
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x == null)
                .And.ContainSingle(x => x != null && x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True));
        }

        [DataTestMethod]
        [DataRow("value > 0")]
        [DataRow("true")]
        public void DoLoop_InstructionVisitedMaxTwice(string condition)
        {
            var code = $@"
var value = 42;
do
{{
    value.ToString(); // Add another constraint to 'value'
    value--;
}} while ({condition});
Tag(""End"", value);";
            var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
            validator.ValidateExitReachCount(2);
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True));
        }

        [TestMethod]
        public void InfiniteLoopWithNoExitBranch_InstructionVisitedMaxTwice()
        {
            const string code = @"
var value = 42;
for( ; ; )
{
    value.ToString(); // Add another constraint to 'value'
    Tag(""Inside"", value);
}";
            var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
            validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block
            validator.TagValues("Inside").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
        }

        [TestMethod]
        public void GoTo_InfiniteWithNoExitBranch_InstructionVisitedMaxTwice()
        {
            const string code = @"
var value = 42;
Start:
value.ToString(); // Add another constraint to 'value'
Tag(""Inside"", value);
goto Start;";
            var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
            validator.ValidateExitReachCount(0);                    // There's no branch to 'Exit' block
            validator.TagValues("Inside").Should().HaveCount(4)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
        }

        [TestMethod]
        public void GoTo_InstructionVisitedMaxTwice()
        {
            const string code = @"
var value = 42;
Start:
value.ToString(); // Add another constraint to 'value'
value--;
if (value > 0)
{
    goto Start;
}
Tag(""End"", value);";
            var validator = SETestContext.CreateCS(code, new AddConstraintOnInvocationCheck()).Validator;
            validator.ValidateExitReachCount(2);
            validator.TagValues("End").Should().HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && !x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(TestConstraint.First) && x.HasConstraint(BoolConstraint.True) && !x.HasConstraint(DummyConstraint.Dummy));
        }

    }
}
