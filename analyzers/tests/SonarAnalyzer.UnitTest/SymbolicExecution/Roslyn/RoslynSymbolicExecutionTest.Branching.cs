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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

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
            SETestContext.CreateCS(code).Collector.ValidateTagOrder(
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
            SETestContext.CreateVB(code).Collector.ValidateTagOrder(
                "Entry",
                "BeforeTry",
                "InTry",
                "InFinally",
                "AfterFinally",
                "Else",
                "End");
        }
    }
}
