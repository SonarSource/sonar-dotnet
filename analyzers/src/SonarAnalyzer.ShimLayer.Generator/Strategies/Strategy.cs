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

public abstract class Strategy
{
    public abstract string Generate(StrategyModel model);
    public abstract string ReturnTypeSnippet();
    public abstract string ToConversionSnippet(string from);

    public virtual bool IsSupported => true;

    public Type Latest { get; }

    protected Strategy(Type latest) =>
        Latest = latest;

    public virtual string CompiletimeTypeSnippet() =>
        Latest.Name;

    public virtual string PropertyAccessorInitializerSnippet(string compiletimeType, string propertyName) =>
        $"LightupHelpers.CreateSyntaxPropertyAccessor<{compiletimeType}, {CompiletimeTypeSnippet()}>(WrappedType, nameof({propertyName}))";

    protected static string SerializeAttributes(IEnumerable<CustomAttributeData> attributes, int indentSize)
    {
        var sb = new StringBuilder();
        var indent = new string(' ', indentSize);
        foreach (var attribute in attributes.Where(x => x.AttributeType.Name is not "ExperimentalAttribute" and not "NullableAttribute"))
        {
            sb.Append("[").Append(attribute.AttributeType.FullName);
            if (attribute.ConstructorArguments.Any())
            {
                sb.Append("(");
                sb.Append(string.Join(", ", attribute.ConstructorArguments.Select(SerializeArgument)));
                sb.Append(")");
            }
            sb.AppendLine("]");
            sb.Append(indent);
        }
        return sb.ToString();
    }

    private static string SerializeArgument(CustomAttributeTypedArgument arg)
    {
        if (arg.ArgumentType.Name == nameof(String))
        {
            return $@"""{arg.Value}""";
        }
        else if (arg.ArgumentType.Name == nameof(Boolean))
        {
            return arg.Value.ToString().ToLower();
        }
        else if (arg.ArgumentType.IsEnum)   // If the Enum is not in Roslyn 1.3.2, or netstandard2.0, consider excluding the entire attribute
        {
            return $"{arg.ArgumentType.FullName}.{Enum.GetName(arg.ArgumentType, arg.Value)}";
        }
        else
        {
            return arg.Value?.ToString() ?? "null";
        }
    }
}
