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

namespace SonarAnalyzer.CFG.Test;

[TestClass]
public class DotWriterTest
{
    [TestMethod]
    public void WriteGraphStart()
    {
        var writer = new DotWriter();
        writer.WriteGraphStart("test");
        writer.ToString().Should().BeIgnoringLineEndings("digraph \"test\" {\r\n");
        writer.Invoking(x => x.WriteGraphStart("second")).Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void WriteGraphEnd()
    {
        var writer = new DotWriter();
        writer.Invoking(x => x.WriteGraphEnd()).Should().Throw<InvalidOperationException>();
        writer.WriteGraphStart("test");
        writer.WriteGraphEnd();
        writer.ToString().Should().BeIgnoringLineEndings("digraph \"test\" {\r\n}\r\n");
    }

    [TestMethod]
    public void WriteSubGraphStart()
    {
        var writer = new DotWriter();
        writer.WriteSubGraphStart(42, "test");
        writer.ToString().Should().BeIgnoringLineEndings("subgraph \"cluster_42\" {\r\nlabel = \"test\"\r\n");
    }

    [TestMethod]
    public void WriteSubGraphEnd()
    {
        var writer = new DotWriter();
        writer.WriteSubGraphEnd();
        writer.ToString().Should().BeIgnoringLineEndings("}\r\n");
    }

    [TestMethod]
    public void WriteNode_WithItems()
    {
        var writer = new DotWriter();
        writer.WriteRecordNode("1", "header", "a", "b", "c");
        writer.ToString().Should().BeIgnoringLineEndings("1 [shape=record label=\"{header|a|b|c}\"]\r\n");
    }

    [TestMethod]
    public void WriteNode_WithEncoding()
    {
        var writer = new DotWriter();
        writer.WriteRecordNode("1", "header", "\r", "\n", "{", "}", "<", ">", "|", "\"");
        writer.ToString().Should().BeIgnoringLineEndings(@"1 [shape=record label=""{header||\n|\{|\}|\<|\>|\||\""}""]" + "\r\n");
    }

    [TestMethod]
    public void WriteNode_NoItems()
    {
        var writer = new DotWriter();
        writer.WriteRecordNode("1", "header");
        writer.ToString().Should().BeIgnoringLineEndings("1 [shape=record label=\"{header}\"]\r\n");
    }
}
