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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.CFG.Sonar
{
    [TestClass]
    public class RoslynCfgSerializerTest
    {
        [TestMethod]
        public void Serialize_MethodNameUsedInTitle()
        {
            var code = @"
class Sample
{
    void Method() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code), "GraphTitle");
            dot.Should().BeIgnoringLineEndings(
@"digraph ""GraphTitle"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{EXIT #1}""]
cfg0_block0 -> cfg0_block1
}
");
        }

        [TestMethod]
        public void Serialize_Empty_Method()
        {
            var code = @"
class Sample
{
    void Method() { }
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{EXIT #1}""]
cfg0_block0 -> cfg0_block1
}
");
        }

        [TestMethod]
        public void Serialize_OperationSequence()
        {
            var code = @"
class Sample
{
    void Method()
    {
        A();
        B();
        var c = C();
    }

    private void A() { }
    private void B() { }
    private int C() => 42;
}";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|0# ExpressionStatementOperation / ExpressionStatementSyntax: A();|1# InvocationOperation: A / InvocationExpressionSyntax: A()|2# InstanceReferenceOperation / IdentifierNameSyntax: A|##########|0# ExpressionStatementOperation / ExpressionStatementSyntax: B();|1# InvocationOperation: B / InvocationExpressionSyntax: B()|2# InstanceReferenceOperation / IdentifierNameSyntax: B|##########|0# SimpleAssignmentOperation / VariableDeclaratorSyntax: c = C()|1# LocalReferenceOperation / VariableDeclaratorSyntax: c = C()|1# InvocationOperation: C / InvocationExpressionSyntax: C()|2# InstanceReferenceOperation / IdentifierNameSyntax: C|##########}""]
cfg0_block0 -> cfg0_block1
cfg0_block2 [shape=record label=""{EXIT #2}""]
cfg0_block1 -> cfg0_block2
}
");
        }

        [TestMethod]
        public void Serialize_Branch_Switch()
        {
            var code = @"
class Sample
{
    void Foo(int a)
    {
        switch (a)
        {
            case 1:
                c1();
                break;

            case 2:
                c2();
                break;
        }
    }
    private void c1();
    private void c2();
}
";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));
            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|0# FlowCaptureOperation / IdentifierNameSyntax: a|1# ParameterReferenceOperation / IdentifierNameSyntax: a|##########|## BranchValue ##|0# BinaryOperation / LiteralExpressionSyntax: 1|1# FlowCaptureReferenceOperation / IdentifierNameSyntax: a|1# LiteralOperation / LiteralExpressionSyntax: 1|##########}""]
cfg0_block0 -> cfg0_block1
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation / ExpressionStatementSyntax: c1();|1# InvocationOperation: c1 / InvocationExpressionSyntax: c1()|2# InstanceReferenceOperation / IdentifierNameSyntax: c1|##########}""]
cfg0_block1 -> cfg0_block2 [label=""Else""]
cfg0_block3 [shape=record label=""{BLOCK #3|## BranchValue ##|0# BinaryOperation / LiteralExpressionSyntax: 2|1# FlowCaptureReferenceOperation / IdentifierNameSyntax: a|1# LiteralOperation / LiteralExpressionSyntax: 2|##########}""]
cfg0_block1 -> cfg0_block3 [label=""WhenFalse""]
cfg0_block4 [shape=record label=""{BLOCK #4|0# ExpressionStatementOperation / ExpressionStatementSyntax: c2();|1# InvocationOperation: c2 / InvocationExpressionSyntax: c2()|2# InstanceReferenceOperation / IdentifierNameSyntax: c2|##########}""]
cfg0_block3 -> cfg0_block4 [label=""Else""]
cfg0_block5 [shape=record label=""{EXIT #5}""]
cfg0_block2 -> cfg0_block5
cfg0_block3 -> cfg0_block5 [label=""WhenFalse""]
cfg0_block4 -> cfg0_block5
}
");
        }

        [TestMethod]
        public void Serialize_Branch_If()
        {
            var code = @"
class Sample
{
    void Method()
    {
        if (true)
        {
            Bar();
        }
    }
    void Bar() { }
}
";
            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            dot.Should().BeIgnoringLineEndings(
@"digraph ""RoslynCfg"" {
cfg0_block0 [shape=record label=""{ENTRY #0}""]
cfg0_block1 [shape=record label=""{BLOCK #1|## BranchValue ##|0# LiteralOperation / LiteralExpressionSyntax: true|##########}""]
cfg0_block0 -> cfg0_block1
cfg0_block2 [shape=record label=""{BLOCK #2|0# ExpressionStatementOperation / ExpressionStatementSyntax: Bar();|1# InvocationOperation: Bar / InvocationExpressionSyntax: Bar()|2# InstanceReferenceOperation / IdentifierNameSyntax: Bar|##########}""]
cfg0_block1 -> cfg0_block2 [label=""Else""]
cfg0_block3 [shape=record label=""{EXIT #3}""]
cfg0_block1 -> cfg0_block3 [label=""WhenFalse""]
cfg0_block2 -> cfg0_block3
}
");
        }

        [TestMethod]
        public void Serialize_Foreach_Binary_Simple()
        {
            var code = @"
class Sample
{
    void Method()
    {
        foreach (var i in items)
        {
            Bar();
        }
    }
}
";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""Foo"" {
            //0 [shape=record label=""{FOREACH:ForEachStatement|items}""]
            //0 -> 1
            //1 [shape=record label=""{BINARY:ForEachStatement}""]
            //1 -> 2 [label=""True""]
            //1 -> 3 [label=""False""]
            //2 [shape=record label=""{SIMPLE|Bar|Bar()}""]
            //2 -> 1
            //3 [shape=record label=""{EXIT}""]
            //}
            //");
        }

        [TestMethod]
        public void Serialize_Foreach_Binary_VarDeclaration()
        {
            var code = @"
namespace Namespace
    {
        public class Test
        {
            public void ForEach((string key, string value)[] values)
            {
                foreach (var (key, value) in values)
                {
                    string i = key;
                    string j = value;
                }
            }
        }
    };";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize("ForEach", GetCfgForMethod(code, "ForEach"));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""ForEach"" {
            //0 [shape=record label=""{FOREACH:ForEachVariableStatement|values}""]
            //0 -> 1
            //1 [shape=record label=""{BINARY:ForEachVariableStatement}""]
            //1 -> 2 [label=""True""]
            //1 -> 3 [label=""False""]
            //2 [shape=record label=""{SIMPLE|key|i = key|value|j = value}""]
            //2 -> 1
            //3 [shape=record label=""{EXIT}""]
            //}
            //");
        }

        [TestMethod]
        public void Serialize_For_Binary_Simple()
        {
            var code = @"
class Sample
{
    void Method()
    {
        for (var i = 0; i < 10; i++)
        {
            Bar();
        }
    }
}
";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""Foo"" {
            //0 [shape=record label=""{FOR:ForStatement|0|i = 0}""]
            //0 -> 1
            //1 [shape=record label=""{BINARY:ForStatement|i|10|i \< 10}""]
            //1 -> 2 [label=""True""]
            //1 -> 3 [label=""False""]
            //2 [shape=record label=""{SIMPLE|Bar|Bar()}""]
            //2 -> 4
            //4 [shape=record label=""{SIMPLE|i|i++}""]
            //4 -> 1
            //3 [shape=record label=""{EXIT}""]
            //}
            //");
        }

        [TestMethod]
        public void Serialize_Jump_Using()
        {
            var code = @"
class Sample
{
    void Method()
    {
        using (x)
        {
            Bar();
        }
    }
}
";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""Foo"" {
            //0 [shape=record label=""{JUMP:UsingStatement|x}""]
            //0 -> 1
            //1 [shape=record label=""{USING:UsingStatement|Bar|Bar()}""]
            //1 -> 2
            //2 [shape=record label=""{EXIT}""]
            //}
            //");
        }

        [TestMethod]
        public void Serialize_Lock_Simple()
        {
            var code = @"
class Sample
{
    void Method()
    {
        lock (x)
        {
            Bar();
        }
    }
}
";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""Foo"" {
            //0 [shape=record label=""{LOCK:LockStatement|x}""]
            //0 -> 1
            //1 [shape=record label=""{SIMPLE|Bar|Bar()}""]
            //1 -> 2
            //2 [shape=record label=""{EXIT}""]
            //}
            //");
        }

        [TestMethod]
        public void Serialize_Lambda()
        {
            var code = @"
class Sample
{
    void Method()
    {
        Bar(x =>
        {
            return 1 + 1;
        });
    }
}
";
            throw new System.NotImplementedException();
            //            var dot = CfgSerializer.Serialize(TestHelper.CompileCfg(code));

            //            dot.Should().BeIgnoringLineEndings(@"digraph ""Foo"" {
            //0 [shape=record label=""{SIMPLE|Bar|x =\>\n        \{\n            return 1 + 1;\n        \}|Bar(x =\>\n        \{\n            return 1 + 1;\n        \})}""]
            //0 -> 1
            //1 [shape=record label=""{EXIT}""]
            //}
            //");
        }
    }
}
