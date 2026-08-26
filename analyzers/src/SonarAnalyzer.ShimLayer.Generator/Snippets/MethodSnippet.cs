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

    public sealed override string MemberDeclaration(int indentSize) =>
        $"""
        {Indent(indentSize)}{SerializeAttributes(member.GetCustomAttributesData(), indentSize)}public {returnType.ReturnTypeSnippet} {member.Name}({parameters.JoinStr(", ", SerializeParameter)}) => {InvocationSnippet()};
        """;

    protected static string SerializeParameterArgument(ParameterInfo parameter)
    {
        var name = SerializeParameterName(parameter);
        return parameter.IsOut ? $"out {name}" : name;
    }

    protected string SerializeParameter(ParameterInfo parameter)
    {
        var prefix = parameter.IsOut ? "out " : null;
        var underlyingType = parameter.IsOut ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
        return $"{prefix}{model[underlyingType].ReturnTypeSnippet} {SerializeParameterName(parameter)}";
    }

    private static string SerializeParameterName(ParameterInfo parameter) =>
        SyntaxFacts.GetKeywordKind(parameter.Name) == SyntaxKind.None ? parameter.Name : $"@{parameter.Name}";
}
