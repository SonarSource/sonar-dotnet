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

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.ShimLayer.Generator.Snippets;

public abstract class MethodSnippet : Snippet<MethodInfo>
{
    protected readonly StrategyModel model;
    protected readonly ParameterInfo[] parameters;

    protected abstract string InvocationSnippet();

    protected MethodSnippet(Strategy strategy, MemberDescriptor member, Strategy returnType, StrategyModel model) : base(strategy, member, returnType)
    {
        this.model = model;
        parameters = this.member.GetParameters();
    }

    public sealed override string MemberDeclaration(int indentSize)
    {
        var attributes = member.GetCustomAttributesData();
        var staticSnippet = member.IsStatic ? "static " : null;
        var thisSnippet = attributes.Any(x => x.AttributeType.Name == nameof(ExtensionAttribute)) ? "this " : null;
        var genericArgumentsSnippet = member.ContainsGenericParameters ? $"<{member.GetGenericArguments().JoinStr(", ", x => x.Name)}>" : null;
        return $"""
            {Indent(indentSize)}{SerializeAttributes(attributes, indentSize)}public {staticSnippet}{returnType.ReturnTypeSnippet} {member.Name}{genericArgumentsSnippet}({thisSnippet}{parameters.JoinStr(", ", SerializeParameter)}){SerializeGenericConstraints()} => {InvocationSnippet()};
            """;
    }

    protected static string SerializeParameterArgument(ParameterInfo parameter)
    {
        var name = SerializeParameterName(parameter);
        return parameter.IsOut ? $"out {name}" : name;
    }

    protected string SerializeParameter(ParameterInfo parameter)
    {
        var underlyingType = parameter.IsOut ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
        return $"{Prefix()}{model[underlyingType].TypeSnippet} {SerializeParameterName(parameter)}";

        string Prefix()
        {
            if (parameter.IsOut)
            {
                return "out ";
            }
            else if (parameter.GetCustomAttributesData().Any(x => x.AttributeType.Name == nameof(ParamArrayAttribute)))
            {
                return "params ";
            }
            else
            {
                return null;
            }
        }
    }

    protected string SerializeGenericConstraints() =>
        member.GetGenericArguments().Select(SerializeGenericConstraint).Where(x => x is not null).JoinStr(" ");

    private static string SerializeGenericConstraint(Type parameter)
    {
        var parts = new List<string>();
        TryAdd(GenericParameterAttributes.ReferenceTypeConstraint, "class");
        TryAdd(GenericParameterAttributes.NotNullableValueTypeConstraint, "struct");
        parts.AddRange(parameter.GetGenericParameterConstraints().Where(x => x.FullName != typeof(ValueType).FullName).Select(TypeName));
        if (!HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))    // struct implies new() and can not be rendered together
        {
            TryAdd(GenericParameterAttributes.DefaultConstructorConstraint, "new()");
        }
        return parts.Any() ? $" where {parameter.Name} : {parts.JoinStr(", ")}" : null;

        bool HasFlag(GenericParameterAttributes flag) =>
            (parameter.GenericParameterAttributes & flag) == flag;

        void TryAdd(GenericParameterAttributes flag, string value)
        {
            if (HasFlag(flag))
            {
                parts.Add(value);
            }
        }

        static string TypeName(Type type) =>
            type.IsGenericType
                ? $"{type.Name.Substring(0, type.Name.IndexOf('`'))}<{string.Join(", ", type.GetGenericArguments().Select(TypeName))}>"
                : type.Name;
    }

    private static string SerializeParameterName(ParameterInfo parameter) =>
        SyntaxFacts.GetKeywordKind(parameter.Name) == SyntaxKind.None ? parameter.Name : $"@{parameter.Name}";
}
