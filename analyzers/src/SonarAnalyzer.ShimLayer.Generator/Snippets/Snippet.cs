/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.ShimLayer.Generator.Snippets;

public abstract class Snippet
{
    public abstract string AccessorDeclaration();
    public abstract string MemberDeclaration(int indentSize);

    public static string SerializeAttributes(IEnumerable<CustomAttributeData> attributes, int indentSize)
    {
        var sb = new StringBuilder();
        var indent = Indent(indentSize);
        foreach (var attribute in attributes.Where(x => x.AttributeType.Name is not "AsyncStateMachineAttribute"
                                                                                and not "ExperimentalAttribute"
                                                                                and not "IteratorStateMachineAttribute"
                                                                                and not "MemberNotNullWhenAttribute"
                                                                                and not "NullableAttribute"
                                                                                and not "NullableContextAttribute"
                                                                                and not "TupleElementNamesAttribute"))
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

    protected static string Indent(int indentSize) =>
        new(' ', indentSize);

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

public abstract class Snippet<TMember> : Snippet where TMember : MemberInfo
{
    protected readonly Strategy strategy;
    protected readonly TMember member;
    protected readonly string accessorName;
    protected readonly Strategy returnType;

    protected Snippet(Strategy strategy, MemberDescriptor member, Strategy returnType)
    {
        this.strategy = strategy;
        this.member = (TMember)member.Member;
        this.accessorName = member.AccessorName;
        this.returnType = returnType;
    }
}
