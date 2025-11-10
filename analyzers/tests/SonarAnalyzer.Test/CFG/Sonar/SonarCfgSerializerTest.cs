/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.CFG.Sonar.Test;

[TestClass]
public class SonarCfgSerializerTest
{
    [TestMethod]
    public void Serialize_Empty_Method()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{EXIT}"]
            }

            """);
    }

    [TestMethod]
    public void Serialize_Branch_Jump()
    {
        const string code = """
            class C
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
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{BRANCH:SwitchStatement|a}"]
            1 [shape=record label="{BINARY:CaseSwitchLabel|a}"]
            2 [shape=record label="{JUMP:BreakStatement|c1|c1()}"]
            3 [shape=record label="{BINARY:CaseSwitchLabel|a}"]
            5 [shape=record label="{JUMP:BreakStatement|c2|c2()}"]
            4 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2 [label="True"]
            1 -> 3 [label="False"]
            2 -> 4
            3 -> 5 [label="True"]
            3 -> 4 [label="False"]
            5 -> 4
            }

            """);
    }

    [TestMethod]
    public void Serialize_BinaryBranch_Simple()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    if (true)
                    {
                        Bar();
                    }
                }
                void Bar() { }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{BINARY:TrueLiteralExpression|true}"]
            1 [shape=record label="{SIMPLE|Bar|Bar()}"]
            2 [shape=record label="{EXIT}"]
            0 -> 1 [label="True"]
            0 -> 2 [label="False"]
            1 -> 2
            }

            """);
    }

    [TestMethod]
    public void Serialize_Foreach_Binary_Simple()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    foreach (var i in items)
                    {
                        Bar();
                    }
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{FOREACH:ForEachStatement|items}"]
            1 [shape=record label="{BINARY:ForEachStatement}"]
            2 [shape=record label="{SIMPLE|Bar|Bar()}"]
            3 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2 [label="True"]
            1 -> 3 [label="False"]
            2 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_Foreach_Binary_VarDeclaration()
    {
        const string code = """
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
            };
            """;

        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "ForEach");

        dot.Should().BeIgnoringLineEndings("""
            digraph "ForEach" {
            0 [shape=record label="{FOREACH:ForEachVariableStatement|values}"]
            1 [shape=record label="{BINARY:ForEachVariableStatement}"]
            2 [shape=record label="{SIMPLE|key|i = key|value|j = value}"]
            3 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2 [label="True"]
            1 -> 3 [label="False"]
            2 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_For_Binary_Simple()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    for (var i = 0; i < 10; i++)
                    {
                        Bar();
                    }
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{FOR:ForStatement|0|i = 0}"]
            1 [shape=record label="{BINARY:ForStatement|i|10|i \< 10}"]
            2 [shape=record label="{SIMPLE|Bar|Bar()}"]
            4 [shape=record label="{SIMPLE|i|i++}"]
            3 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2 [label="True"]
            1 -> 3 [label="False"]
            2 -> 4
            4 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_Jump_Using()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    using (x)
                    {
                        Bar();
                    }
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{JUMP:UsingStatement|x}"]
            1 [shape=record label="{USING:UsingStatement|Bar|Bar()}"]
            2 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2
            }

            """);
    }

    [TestMethod]
    public void Serialize_Lock_Simple()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    lock (x)
                    {
                        Bar();
                    }
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{LOCK:LockStatement|x}"]
            1 [shape=record label="{SIMPLE|Bar|Bar()}"]
            2 [shape=record label="{EXIT}"]
            0 -> 1
            1 -> 2
            }

            """);
    }

    [TestMethod]
    public void Serialize_Lambda()
    {
        const string code = """
            class C
            {
                void Foo()
                {
                    Bar(x =>
                    {
                        return 1 + 1;
                    });
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Foo");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Foo" {
            0 [shape=record label="{SIMPLE|Bar|x =\>\n        \{\n            return 1 + 1;\n        \}|Bar(x =\>\n        \{\n            return 1 + 1;\n        \})}"]
            1 [shape=record label="{EXIT}"]
            0 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_Range()
    {
        const string code = """
            internal class Test
            {
                public void Range()
                {
                    Range r = 1..4;
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Range");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Range" {
            0 [shape=record label="{SIMPLE|1..4|r = 1..4}"]
            1 [shape=record label="{EXIT}"]
            0 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_Index()
    {
        const string code = """
            internal class Test
            {
                public void Index()
                {
                    Index index = ^1;
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Index");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Index" {
            0 [shape=record label="{SIMPLE|^1|index = ^1}"]
            1 [shape=record label="{EXIT}"]
            0 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_IndexInRange()
    {
        const string code = """
            internal class Test
            {
                public void Range()
                {
                    Range range = ^2..^0;
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Range");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Range" {
            0 [shape=record label="{SIMPLE|^2..^0|range = ^2..^0}"]
            1 [shape=record label="{EXIT}"]
            0 -> 1
            }

            """);
    }

    [TestMethod]
    public void Serialize_RangeInIndexer()
    {
        const string code = """
            internal class Test
            {
                public void Range()
                {
                    var ints = new[] { 1, 2 };
                    var lastTwo = ints[^2..^1];
                }
            }
            """;
        var dot = CfgSerializer.Serialize(CreateMethodCfg(code), "Range");

        dot.Should().BeIgnoringLineEndings("""
            digraph "Range" {
            0 [shape=record label="{SIMPLE|new[] \{ 1, 2 \}|1|2|\{ 1, 2 \}|ints = new[] \{ 1, 2 \}|ints|^2..^1|ints[^2..^1]|lastTwo = ints[^2..^1]}"]
            1 [shape=record label="{EXIT}"]
            0 -> 1
            }

            """);
    }

    private static IControlFlowGraph CreateMethodCfg(string code)
    {
        var (tree, model) = TestCompiler.CompileIgnoreErrorsCS(code);
        return CSharpControlFlowGraph.Create(tree.First<MethodDeclarationSyntax>().Body, model);
    }
}
