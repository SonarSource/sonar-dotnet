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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf.Ucfg;
using SonarAnalyzer.UnitTest.Security.Framework;

namespace SonarAnalyzer.UnitTest.Security.Ucfg
{
    [TestClass]
    public class UcfgBuilder_Blocks
    {
        private const string ConstValue = "\"\"";

        [TestMethod]
        public void Void_Method_Simple()
        {
            const string code = @"
using System;
public class Class1
{
    public void Foo(string s)
    {
        Console.WriteLine(s);       // Simple (Next:Exit)       | Block#0(Jump:#1)
                                    // Exit                     | Block#1(Ret)
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1" }),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void Void_Method_Branch1()
        {
            const string code = @"
using System;
public class Class1
{
    public void Foo(string s)
    {
        if (true)                   // Branch (Next:Jump,Exit)  | Block#0(Jump:#1,#2)
            return;                 // Jump   (Next:Exit)       | Block#1(Ret)
                                    // Exit                     | Block#2(Ret)
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: ConstValue),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void Void_Method_Branch2()
        {
            const string code = @"
using System;
public class Class1
{
    public void Foo(string s)
    {
        if (true)                   // Branch (Next:Jump,Exit)  | Block#0(Jump:#1,#2)
            return;                 // Jump   (Next:Exit)       | Block#1(Ret)
        Console.WriteLine(s);       // Simple (Next:Exit)       | Block#2(Jump:#3)
                                    // Exit                     | Block#3(Ret)
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: ConstValue),
                b => ValidateJmpBlock(b, expectedId: "2", expectedJumps: "3"),
                b => ValidateRetBlock(b, expectedId: "3", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void String_Method_Branch()
        {
            const string code = @"
using System;
public class Class1
{
    public string Foo(string s)
    {
        if (true)                   // Branch (Next:Jump,Exit)  | Block#0(Jump:#1,#2)
            return s;               // Jump   (Next:Exit)       | Block#1(Ret:s)
        Console.WriteLine(s);       // Jump   (Next:Exit)       | Block#2(Ret:s)
        return s;
                                    // Exit                     | Block#3(Ret:Const) // ignored when deserializing
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: "s"),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: "s"),
                b => ValidateRetBlock(b, expectedId: "3", expectedReturnExpression: ConstValue));
        }

        [TestMethod]
        public void String_Method_Simple()
        {
            const string code = @"
public class Class1
{
    public string Foo(string s)
    {
        return s;       // Jump   (Next:Exit)       | Block#0(Ret:s)
                        // Exit                     | Block#1(Ret:Const)// ignored when deserializing
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateRetBlock(b, expectedId: "0", expectedReturnExpression: "s"),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: ConstValue));
        }

        [TestMethod]
        public void Throw_Exception()
        {
            const string code = @"
public class Class1
{
    public string Foo(string s)
    {
        if (true)                       // Branch(Jump,Exit)    |   Basic#0(Jump:#1,#2)
        {
            throw new Exception();      // Jump(Exit)           |   Basic#1(Jump:#3)
        }
        return s;                       // Jump(Exit)           |   Basic#2(Ret:s)
    }                                   // Exit                 |   Basic#3(Ret:Const)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "0" });

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2" }),
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: "3"),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: "s"),
                b => ValidateRetBlock(b, expectedId: "3", expectedReturnExpression: ConstValue));
        }

        [TestMethod]
        public void SpecialBlocks_Using_Statement()
        {
            const string code = @"
public class Class1
{
    public void Foo(Func<IDisposable> factory)
    {
        using (var x = factory())   // Jump(Next:UsingEnd)      | Basic#0(Jump:#1)
        {
                                    // UsingEnd(Next:Exit)      | Basic#1(Jump:#2)
        }
                                    // Exit                     | Basic#2(Ret:Const)
    }
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1" }),
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: new[] { "2" }),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void SpecialBlocks_Goto()
        {
            const string code = @"
public class Class1
{
    public void Foo(int x)
    {
        switch (x)              // Branch(Jump0,Jump1,Exit)     |   Basic#0(Jump:#1,#2,#3)
        {
            case 0:             // Jump0(Jump1)                 |   Basic#1(Jump:#2)
                goto case 1;
            case 1:             // Jump1(Exit)                  |   Basic#2(Jump:#3)
                break;
        }
    }                           // Exit                         |   Basic#3(Ret)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", "2", "3" }),
                b => ValidateJmpBlock(b, expectedId: "2", expectedJumps: new[] { "3" }),
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: new[] { "2" }),
                b => ValidateRetBlock(b, expectedId: "3", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void SpecialBlocks_Foreach()
        {
            const string code = @"
public class Class1
{
    public void Foo(string[] items)
    {
        foreach (var item in items)     // Foreach(BinaryBranch)            |   Basic#0
                                        // BinaryBranch(BinaryBranch,Exit)  |   Basic#1(Jump:#1,#2)
        {
        }
    }                                   // Exit                             |   Basic#2(Ret)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");
            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1" }),
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: new[] { "1", "2" }),
                b => ValidateRetBlock(b, expectedId: "2", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void SpecialBlocks_For()
        {
            const string code = @"
public class Class1
{
    public void Foo(string[] items)
    {
        for (int i = 0; i < items.Length; i++)      // For(BinaryBranch)                |   Basic#0(Jump:#1)
                                                    // BinaryBranch(Simple,Exit)        |   Basic#1(Jump:#2,#3)
        {                                           // Simple(BinaryBranch)             |   Basic#2(Jump:#1)
        }
    }                                               // Exit                             |   Basic#3(Ret)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");
            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1", }),
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: new[] { "2", "3" }),
                b => ValidateJmpBlock(b, expectedId: "2", expectedJumps: new[] { "1" }),
                b => ValidateRetBlock(b, expectedId: "3", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void SpecialBlocks_Lock()
        {
            const string code = @"
public class Class1
{
    public void Foo(object o, string s)
    {
        lock (o)        // Lock(Exit)       | Basic#0(Jump:#1)
        {
        }
    }                   // Exit             | Basic#1(Ret)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateJmpBlock(b, expectedId: "0", expectedJumps: new[] { "1" }),
                b => ValidateRetBlock(b, expectedId: "1", expectedReturnExpression: ConstValue)
                );
        }

        [TestMethod]
        public void EntryPointMethod_Has_Additional_Block()
        {
            const string code = @"
using System.Web.Mvc;
public class Class1 : Controller
{
    public void Foo(string s)
    {                   //                  | Basic#1(Jump:#0) - contains entrypoint instruction and attributes
    }                   // Exit             | Basic#0(Ret)
}";
            var ucfg = UcfgVerifier.GetUcfgForMethod(code, "Foo");

            ucfg.Entries.Should().BeEquivalentTo(new[] { "1" });
            TestHelper.AssertCollection(ucfg.BasicBlocks,
                b => ValidateRetBlock(b, expectedId: "0", expectedReturnExpression: ConstValue), // block from cfg
                b => ValidateJmpBlock(b, expectedId: "1", expectedJumps: new[] { "0" }) // fake entrypoint block
                );
        }

        private void ValidateJmpBlock(BasicBlock block, string expectedId, params string[] expectedJumps)
        {
            block.Id.Should().Be(expectedId);
            block.Jump.Should().NotBeNull();
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
    }
}
