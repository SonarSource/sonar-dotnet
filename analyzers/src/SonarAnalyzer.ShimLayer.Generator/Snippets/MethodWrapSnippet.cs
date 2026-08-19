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

public sealed class MethodWrapSnippet : Snippet<MethodInfo>
{
    private readonly StrategyModel model;
    private readonly ParameterInfo[] parameters;

    public MethodWrapSnippet(Strategy strategy, MemberDescriptor member, Strategy returnType, StrategyModel model) : base(strategy, member, returnType)
    {
        this.model = model;
        parameters = this.member.GetParameters();
    }

    public override string AccessorDeclaration()
    {
        var types = ((string[])[strategy.CompiletimeTypeSnippet(), .. parameters.Select(x => model[x.ParameterType].CompiletimeTypeSnippet()), returnType.CompiletimeTypeSnippet()]).JoinStr(", ");
        return $"""    private static readonly Func<{types}> {accessorName} = AccessorFactory.CreateMethod<Func<{types}>>(WrappedType, "{member.Name}");""";
    }

    public override string MemberDeclaration(int indentSize) =>
        $"""
        {Indent(indentSize)}public {returnType.ReturnTypeSnippet()} {member.Name}({parameters.JoinStr(", ", SerializeParameter)}) => {returnType.ToConversionSnippet($"{accessorName}({ parameters.Select(SerializeParameterName).Prepend("wrappedInstance").JoinStr(", ")})")};
        """;

    private string SerializeParameter(ParameterInfo parameter) =>
        $"{model[parameter.ParameterType].CompiletimeTypeSnippet()} {SerializeParameterName(parameter)}";

    private static string SerializeParameterName(ParameterInfo parameter) =>
        SyntaxFacts.GetKeywordKind(parameter.Name) == SyntaxKind.None ? parameter.Name : $"@{parameter.Name}";
}
