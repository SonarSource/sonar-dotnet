/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using SonarAnalyzer.Core.Json.Parsing;

namespace SonarAnalyzer.Core.Json;

public class JsonWalker
{
    protected JsonWalker() { }

    public virtual void Visit(JsonNode node)
    {
        switch (node.Kind)
        {
            case Kind.Object:
                VisitObject(node);
                break;
            case Kind.List:
                VisitList(node);
                break;
            case Kind.Value:
                VisitValue(node);
                break;
        }
    }

    protected virtual void VisitObject(JsonNode node)
    {
        foreach (var key in node.Keys)
        {
            VisitObject(key, node[key]);
        }
    }

    protected virtual void VisitObject(string key, JsonNode value) =>
        Visit(value);

    protected virtual void VisitList(JsonNode node)
    {
        foreach (var item in node)
        {
            Visit(item);
        }
    }

    protected virtual void VisitValue(JsonNode node)
    {
        // Override me
    }
}
