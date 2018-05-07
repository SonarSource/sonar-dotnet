/*
 * SonarAnalyzer for .NET
  Copyright (C) 2015-2018 SonarSource SA
  mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
  modify it under the terms of the GNU Lesser General Public
  License as published by the Free Software Foundation; either
  version 3 of the License, or (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
  Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
  along with this program; if not, write to the Free Software Foundation,
  Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

extern alias csharp;
using System;
using System.Collections.Generic;
using csharp::SonarAnalyzer.Security.Ucfg;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf.Ucfg;
using csharp::SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UniversalControlFlowGraphBuilder_Blocks
    {

        [TestMethod]
        public void VoidMethod()
        {
            const string code = @"
namespace Namespace
{
    using System;
    public class Class1
    {
        public void Foo(string s)
        {
            if (true)                   // Branch (Next:Jump,Simple)     |   Basic0(Jump:1,2)
                return;                 // Jump   (Next:End)             |   Basic1(Jump:Exit)
            Console.WriteLine(s);       // Simple (Next:End)             |   Basic2(Jump:Exit)
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            AssertCollection(ucfg.BasicBlocks,
                b => ValidateJumpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "1"),
                b => ValidateRetBlock(b, expectedId: "2")
                );
        }

        [TestMethod]
        public void StringMethod()
        {
            const string code = @"
namespace Namespace
{
    using System;
    public class Class1
    {
        public string Foo(string s)
        {
            if (true)                   // Branch (Next:Jump,Simple)     |   Basic0(Jump:1,2)
                return string.Empty;    // Jump   (Next:End)             |   Basic1(Ret:Const)
            return s;                   // Jump   (Next:End)             |   Basic2(Ret:s)
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            AssertCollection(ucfg.BasicBlocks,
                b => ValidateJumpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "1"),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: "s"));
        }

        [TestMethod]
        public void StringMethod_Simple_Return()
        {
            const string code = @"
public class Class1
{
    public string Foo(string s)
    {
        return s;                       // Jump   (Next:End)    |   Basic0(Ret:s)
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            AssertCollection(ucfg.BasicBlocks,
                b => ValidateRetBlock(b, expectedId: "0", expectedReturnExpression: "s"));
        }

        [TestMethod]
        public void StringMethod_Switch()
        {
            const string code = @"
public class Class1
{
    public string Foo(string s)
    {
        switch (s.Length)
        {
            case 1:
                return s;
            default:
                return string.Empty;
        }
    }
}";
            var ucfg = GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            AssertCollection(ucfg.BasicBlocks,
                b => ValidateJumpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: "\"\""),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: "s")
                );
        }

        private static void AssertCollection<T>(IList<T> items, params Action<T>[] asserts)
        {
            items.Should().HaveSameCount(asserts);
            for (var i = 0; i < items.Count; i++)
            {
                asserts[i](items[i]);
            }
        }

        private void ValidateJumpBlock(BasicBlock block, string expectedId, params string[] expectedJumps)
        {
            block.Id.Should().Be(expectedId);
            if (expectedJumps.Length > 0)
            {
                block.Jump.Destinations.ShouldBeEquivalentTo(expectedJumps, o => o.WithStrictOrdering());
            }
            else
            {
                block.Jump.Destinations.Should().BeEmpty();
            }
        }

        private void ValidateRetBlock(BasicBlock block, string expectedId, string expectedReturnExpression = null)
        {
            block.Id.Should().Be(expectedId);
            block.Ret.Should().NotBeNull();
            if (expectedReturnExpression == null)
            {
                block.Ret.ReturnedExpression.Const?.Value.Should().Be("\"\"");
            }
            else
            {
                block.Ret.ReturnedExpression.Var?.Name.Should().Be(expectedReturnExpression);
            }
        }

        private UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code).GetMethod(methodName);

            var builder = new UniversalControlFlowGraphBuilder(semanticModel,
                CSharpControlFlowGraph.Create(method.Body, semanticModel));

            return builder.Build(method, semanticModel.GetDeclaredSymbol(method));
        }
    }
}
