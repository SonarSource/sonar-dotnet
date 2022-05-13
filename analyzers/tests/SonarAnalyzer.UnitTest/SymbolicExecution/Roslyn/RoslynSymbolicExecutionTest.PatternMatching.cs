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
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void IsPattern_TrueConstraint_SwitchExpressionFromSymbol_VisitsOnlyOneBranch()
        {
            const string code = @"
var isTrue = true;
var value = isTrue switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void IsPattern_TrueConstraint_SwitchExpressionFromOperation_VisitsOnlyOneBranch()
        {
            const string code = @"
var isTrue = true;
var value = (isTrue == true) switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
        }

        [TestMethod]
        public void IsPattern_FalseConstraint_SwitchExpressionFromSymbol_VisitsOnlyOneBranch()
        {
            const string code = @"
var isFalse = false;
var value = isFalse switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
            SETestContext.CreateCS(code).Validator.ValidateTag("Value", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void IsPattern_NoConstraint_SwitchExpression_VisitsBothBranches()
        {
            const string code = @"
var value = boolParameter switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
            SETestContext.CreateCS(code).Validator.TagValues("Value").Should()
                .HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
        }

        [TestMethod]
        public void IsPattern_OtherConstraint_SwitchExpression_VisitsBothBranches()
        {
            const string code = @"
var value = boolParameter switch
{
    true => true,
    false => false
};
Tag(""Value"", value);";
            var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKind.ParameterReference ? x.SetOperationConstraint(DummyConstraint.Dummy) : x.State);
            SETestContext.CreateCS(code, check).Validator.TagValues("Value").Should()
                .HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
        }

        [TestMethod]
        public void IsPattern_OtherPattern_VisitsBothBranches()
        {
            const string code = @"
var value = arg switch
{
    string => true,
    null => true,
    _ => false
};
Tag(""Value"", value);";
            var check = new PostProcessTestCheck(x => x.Operation.Instance.Kind == OperationKind.ParameterReference ? x.SetOperationConstraint(DummyConstraint.Dummy) : x.State);
            SETestContext.CreateCS(code, ", object arg", check).Validator.TagValues("Value").Should()
                .HaveCount(2)
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.True))
                .And.ContainSingle(x => x.HasConstraint(BoolConstraint.False));
        }
    }
}
