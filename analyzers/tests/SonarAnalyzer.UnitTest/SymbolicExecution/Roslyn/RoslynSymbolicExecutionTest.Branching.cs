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
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Branching_IterateBlocksOrdinal_CS()    // ToDo: This is a temporary simplification until we support proper branching.
        {
            const string code = @"
Tag(""Entry"");
if (boolParameter)
{
    Tag(""BeforeTry"");
    try
    {
        Tag(""InTry"");
    }
    catch
    {
        Tag(""InCatch"");
    }
    finally
    {
        Tag(""InFinally"");
    }
    Tag(""AfterFinally"");
}
else
{
    Tag(""Else"");
}
Tag(""End"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "Entry",
                "BeforeTry",
                "InTry",
                "InCatch",
                "InFinally",
                "AfterFinally",
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_IterateBlocksOrdinal_VB()    // ToDo: This is a temporary simplification until we support proper branching.
        {
            const string code = @"
Tag(""Entry"")
If BoolParameter Then
    Try
        Tag(""BeforeTry"")
    Catch
        Tag(""InTry"")
    Finally
        Tag(""InFinally"")
    End Try
    Tag(""AfterFinally"")
Else
    Tag(""Else"")
End If
Tag(""End"")";
            SETestContext.CreateVB(code).Validator.ValidateTagOrder(
                "Entry",
                "BeforeTry",
                "InTry",
                "InFinally",
                "AfterFinally",
                "Else",
                "End");
        }

        [TestMethod]
        public void Branching_PersistSymbols_BetweenBlocks()
        {
            const string code = @"
var first = true;
var second = false;
if (boolParameter)
{
    Tag(""IfFirst"", first);
    Tag(""IfSecond"", second);
}
else
{
    Tag(""ElseFirst"", first);
    Tag(""ElseSecond"", second);
}";
            var validator = SETestContext.CreateCS(code, new BoolTestCheck()).Validator;
            validator.ValidateTag("IfFirst", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("IfSecond", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
            validator.ValidateTag("ElseFirst", x => x.HasConstraint(BoolConstraint.True).Should().BeTrue());
            validator.ValidateTag("ElseSecond", x => x.HasConstraint(BoolConstraint.False).Should().BeTrue());
        }

        [TestMethod]
        public void EndNotifications_SimpleFlow()
        {
            var validator = SETestContext.CreateCS("var a = true;").Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_MaxStepCountReached()
        {
            // var x = true; produces 3 operations
            var code = Enumerable.Range(1, RoslynSymbolicExecution.MaxStepCount / 3 + 1).Select(x => $"var x{x} = true;").JoinStr(Environment.NewLine);
            var validator = SETestContext.CreateCS(code).Validator;
            validator.ValidateExitReachCount(0);
            validator.ValidateExecutionNotCompleted();
        }


        [TestMethod]
        public void EndNotifications_MultipleBranches()
        {
            const string method = @"
public int Method(bool a)
{
    if (a)
        return 1;
    else
        return 2;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_Throw()
        {
            const string method = @"
public int Method(bool a)
{
    if (a)
        throw new System.NullReferenceException();
    else
        return 2;
}";
            var returnNotReached = new PreProcessTestCheck(x =>
            {
               x.Operation.Instance.Kind.Should().NotBe(OperationKind.Literal, "we don't support multiple branches yet");
               return x.State;
            });
            var validator = SETestContext.CreateCSMethod(method, returnNotReached).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_YieldReturn()
        {
            const string method = @"
public System.Collections.Generic.IEnumerable<int> Method(bool a)
{
    if (a)
        yield return 1;

    yield return 2;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }

        [TestMethod]
        public void EndNotifications_YieldBreak()
        {
            const string method = @"
public System.Collections.Generic.IEnumerable<int> Method(bool a)
{
    if (a)
        yield break;

    var b = a;
}";
            var validator = SETestContext.CreateCSMethod(method).Validator;
            validator.ValidateExitReachCount(1);
            validator.ValidateExecutionCompleted();
        }
    }
}
