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
    public FieldInfo[] Fields { get; }

    public PartialEnumStrategy(Type latest, FieldInfo[] fields) : base(latest) =>
        Fields = fields;

    public override string Generate(StrategyModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"using {Latest.Namespace};");
        sb.AppendLine();
        sb.AppendLine("namespace SonarAnalyzer.ShimLayer;");
        sb.AppendLine();
        sb.AppendLine($"public static class {Latest.Name}Ex");
        sb.AppendLine("{");
        foreach (var field in Fields)
        {
            sb.AppendLine($"    public const {Latest.Name} {field.Name} = ({Latest.Name}){field.GetRawConstantValue()};");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public override string ReturnTypeSnippet() =>
        Latest.Name;

    public override string ToConversionSnippet(string from) =>
        $"({Latest.Name}){from}";
}
