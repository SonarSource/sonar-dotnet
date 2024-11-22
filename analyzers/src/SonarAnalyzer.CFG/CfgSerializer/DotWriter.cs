/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Text;

namespace SonarAnalyzer.CFG;

public class DotWriter
{
    private readonly StringBuilder builder = new StringBuilder();
    private readonly StringBuilder edges = new StringBuilder();
    private bool started;

    public void WriteGraphStart(string graphName)
    {
        if (started)
        {
            throw new InvalidOperationException("Graph was already started");
        }
        started = true;
        builder.AppendLine($"digraph \"{Encode(graphName)}\" {{");
    }

    public void WriteGraphEnd()
    {
        if (!started)
        {
            throw new InvalidOperationException("Graph was not started");
        }
        started = false;
        builder.Append(edges).AppendLine("}");  // Edges crossing subgraphs must be listed at the end of the main graph to keep nodes rendered in the correct subgraph
        edges.Clear();
    }

    public void WriteSubGraphStart(int id, string title) =>
        builder.AppendLine($"subgraph \"cluster_{id}\" {{\nlabel = \"{Encode(title)}\"");

    public void WriteSubGraphEnd() =>
        builder.AppendLine("}");

    public void WriteRecordNode(string id, string header, params string[] items)
    {
        // Curly braces in the label reverse the orientation of the columns/rows
        // Columns/rows are created with pipe
        // New lines are inserted with \n; \r\n does not work well.
        // ID [shape=record label="{<header>|<line1>\n<line2>\n...}"]
        builder.Append(id).Append(" [shape=record label=\"{").Append(header);
        foreach (var item in items)
        {
            builder.Append("|").Append(Encode(item));
        }
        builder.AppendLine("}\"]");
    }

    public void WriteNode(string id, string[] attributes)
    {
        builder.Append(id);
        if (attributes.Length > 0)
        {
            builder.Append(" [").Append(string.Join(" ", attributes)).Append("]");
        }
        builder.AppendLine();
    }

    public void WriteEdge(string startId, string endId, string label)
    {
        edges.Append(startId).Append(" -> ").Append(endId);
        if (!string.IsNullOrEmpty(label))
        {
            edges.Append($" [label=\"{label}\"]");
        }
        edges.AppendLine();
    }

    public override string ToString() =>
        builder.ToString();

    private static string Encode(string s) =>
        s.Replace("\r", string.Empty)
            .Replace("\n", @"\n")
            .Replace("{", @"\{")
            .Replace("}", @"\}")
            .Replace("|", @"\|")
            .Replace("<", @"\<")
            .Replace(">", @"\>")
            .Replace("\"", @"\""");
}
