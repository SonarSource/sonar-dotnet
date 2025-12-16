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

public class NewEnumStrategy : Strategy
{
    public FieldInfo[] Fields { get; }

    public NewEnumStrategy(Type latest, FieldInfo[] fields) : base(latest) =>
        Fields = fields;

    public override string Generate(StrategyModel model)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Preamble());
        sb.AppendLine($"{SerializeAttributes(Latest.GetCustomAttributesData(), 0)}public enum {Latest.Name} : {Enum.GetUnderlyingType(Latest)}");
        sb.AppendLine("{");
        foreach (var field in Fields)
        {
            sb.AppendLine($"    {field.Name} = {field.GetRawConstantValue()},");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public override string ReturnTypeSnippet() =>
        Latest.Name;

    public override string ToConversionSnippet(string from) =>
        $"({Latest.Name}){from}";
}
