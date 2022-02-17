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
Tag(""AfterFinally"");
";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InTry",
                "InFinally",
                "AfterFinally");
        }

        [TestMethod]
        public void Finally_Nested()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InOuterTry"");
    try
    {
        Tag(""InNestedTry"");
    }
    finally
    {
        Tag(""InNestedFinally"");
    }
}
finally
{
    Tag(""InFinally"");
}
Tag(""AfterFinally"");
";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InOuterTry",
                "InNestedTry",
                "InNestedFinally",
                "InFinally",
                "AfterFinally");
        }

        [TestMethod]
        public void Finally_Nested_InstructionAfterFinally()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InFirstTry"");
    try
    {
        Tag(""InNestedTry"");
    }
    finally
    {
        true.ToString();
        Tag(""InNestedFinally"");
    }
    Tag(""AfterNestedFinally"");
}
finally
{
    Tag(""InFinally"");
}
Tag(""AfterFinally"");
";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InFirstTry",
                "InNestedTry",
                "InNestedFinally",
                "AfterNestedFinally",
                "InFinally",
                "AfterFinally");
        }

        [TestMethod]
        public void Finally_BranchInNested()
        {
            const string code = @"
Tag(""BeforeTry"");
try
{
    Tag(""InFirstTry"");
    try
    {
        Tag(""InNestedTry"");
        if (1 == 2)
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
        Tag(""InNestedFinally"");
    }
    Tag(""AfterNestedFinally"");
}
finally
{
    Tag(""InFinally"");
}
Tag(""AfterFinally"");
";
            SETestContext.CreateCS(code).Validator.ValidateTagOrder(
                "BeforeTry",
                "InFirstTry",
                "InNestedTry",
                "InNestedFinally",
                "1",
                "2",
                "InFinally",
                "AfterNestedFinally",
                "AfterFinally");
        }
    }
}
