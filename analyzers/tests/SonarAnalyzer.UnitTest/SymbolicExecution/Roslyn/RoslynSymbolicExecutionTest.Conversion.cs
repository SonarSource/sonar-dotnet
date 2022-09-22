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
        public void Conversion_Cast_DownCast()
        {
            var code = @"
var result = (ArgumentException)exception;
Tag(""End"");
";
            var validator = SETestContext.CreateCS(code, ", Exception exception").Validator;
            var result = validator.Symbol("result");
            var exception = validator.Symbol("exception");
            validator.TagStates("End").Should().SatisfyRespectively(
                x =>
                {
                    x[result].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[exception].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                },
                x =>
                {
                    x[result].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[exception].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                });
        }

        [TestMethod]
        public void Conversion_TryCast_DownCast()
        {
            var code = @"
var result = exception as ArgumentException;
Tag(""End"");
";
            var validator = SETestContext.CreateCS(code, ", Exception exception").Validator;
            var result = validator.Symbol("result");
            var exception = validator.Symbol("exception");
            validator.TagStates("End").Should().SatisfyRespectively(
                x =>
                {
                    x[result].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                    x[exception].HasConstraint(ObjectConstraint.NotNull).Should().BeTrue();
                },
                x =>
                {
                    x[result].HasConstraint(ObjectConstraint.Null).Should().BeTrue();
                    x[exception].Should().BeNull();
                });
        }

        [TestMethod]
        public void Conversion_TryCast_UpCast()
        {
            var code = @"
var result = argumentException as Exception;
Tag(""End"");
";
            var validator = SETestContext.CreateCS(code, ", ArgumentException argumentException").Validator;
            var result = validator.Symbol("result");
            var exception = validator.Symbol("argumentException");
            validator.TagStates("End").Should().SatisfyRespectively(
                x =>
                {
                    x[result].Should().BeNull();
                    x[exception].Should().BeNull();
                });
        }
    }
}
