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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class RoslynSymbolicExecutionTest
    {
        [TestMethod]
        public void Finally_Simple()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    Tag(""InFinally"");
}
Tag(""AfterFinally"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFinally",
                "AfterFinally");
        }

        [TestMethod]
        public void Finally_Nested_ExitingTwoFinallyOnSameBranch()
        {
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
    }
    finally
    {
        true.ToString();    // Put some operations in the way
        Tag(""InInnerFinally"");
    }
}
finally
{
    true.ToString();    // Put some operations in the way
    true.ToString();
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeOuterTry",
                "InOuterTry",
                "InInnerTry",
                "AfterOuterFinally",    // FIXME: Incorrect order, should be third
                "InInnerFinally",       // FIXME: Incorrect order, should be first
                "InOuterFinally");      // FIXME: Incorrect order, should be second
        }

        [TestMethod]
        public void Finally_Nested_InstructionAfterFinally()
        {
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
    }
    finally
    {
        true.ToString();    // Put some operations in the way
        Tag(""InInnerFinally"");
    }
    Tag(""AfterInnerFinally"");
}
finally
{
    true.ToString();    // Put some operations in the way
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeOuterTry",
                "InOuterTry",
                "InInnerTry",
                "AfterInnerFinally",    // FIXME: Wrong order, should be one below
                "InInnerFinally",       // FIXME: Wrong order, should be one above
                "AfterOuterFinally",    // FIXME: Wrong order, should be one below
                "InOuterFinally");      // FIXME: Wrong order, should be one above
        }

        [TestMethod]
        public void Finally_BranchInNested()
        {
            const string code = @"
Tag(""BeforeOuterTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InInnerTry"");
        if (boolParameter)
        {
            Tag(""1"");
        }
        else
        {
            Tag(""2"");
        }
    }
    finally
    {
        Tag(""InInnerFinally"");
    }
}
finally
{
    Tag(""InOuterFinally"");
}
Tag(""AfterOuterFinally"");";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeOuterTry",
                "InOuterTry",
                "InInnerTry",
                "1",
                "2",
                "InInnerFinally",
                "InOuterFinally",
                "AfterOuterFinally");
        }

        [TestMethod]
        public void Finally_BranchAfterFinally()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InTry"");
}
finally
{
    true.ToString();    // Put some operations in the way
    Tag(""InFinally"");
}
if (boolParameter)  // No operation between the finally and this. This will create a single follow up block with BranchValue
{
    Tag(""1"");
}
else
{
    Tag(""2"");
}";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "1",
                "2",
                "InFinally");    // FIXME: Wrong order, should be before "1" and "2"
        }
    }
}
