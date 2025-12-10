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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies;

public class PartialEnumStrategy : Strategy
{
    private readonly Type latest;

    public FieldInfo[] Fields { get; }

    public PartialEnumStrategy(Type latest, FieldInfo[] fields)
    {
        this.latest = latest;
        Fields = fields;
    }

    public override string Generate(IReadOnlyDictionary<Type, Strategy> model)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"using {latest.Namespace};");
        sb.AppendLine();
        sb.AppendLine("namespace SonarAnalyzer.ShimLayer;");
        sb.AppendLine();
        sb.AppendLine($"public static class {latest.Name}Ex");
        sb.AppendLine("{");
        foreach (var field in Fields)
        {
            sb.AppendLine($"    public const {latest.Name} {field.Name} = ({latest.Name}){field.GetRawConstantValue()};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
