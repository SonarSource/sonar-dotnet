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

public class SyntaxNodeStrategy : Strategy
{
    public Type BaseType { get; }
    public IReadOnlyList<MemberDescriptor> Members { get; }

    public SyntaxNodeStrategy(Type latest, Type baseType, IReadOnlyList<MemberDescriptor> members) : base(latest)
    {
        BaseType = baseType;
        Members = members;
    }

    public override string ReturnTypeSnippet() =>
        $"{Latest.Name}Wrapper";

    public override string ToConversionSnippet(string from) =>
        $"({Latest.Name}Wrapper){from}";

    public override string CompiletimeTypeSnippet() =>
        BaseType.Name;

    public override string Generate(StrategyModel model) =>
        $$"""
        using System;
        using System.Collections.Immutable;
        using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using Microsoft.CodeAnalysis.CSharp.Syntax;
        using Microsoft.CodeAnalysis.Text;

        namespace SonarAnalyzer.ShimLayer;

        public readonly partial struct {{Latest.Name}}Wrapper: ISyntaxWrapper<{{CompiletimeTypeSnippet()}}>
        {
            public const string WrappedTypeName = "{{Latest.FullName}}";
            private static readonly Type WrappedType;

            private readonly {{CompiletimeTypeSnippet()}} node;

            static {{Latest.Name}}Wrapper()
            {
                WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof({{Latest.Name}}Wrapper));
        {{string.Join("\n", Members.Where(x => !x.IsPassthrough).Select(x => MemberAccessorInitialization(x.Member, model)))}}
            }

            private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} node) =>
                this.node = node;

            [Obsolete]
            public {{CompiletimeTypeSnippet()}} SyntaxNode => this.node;

        {{string.Join("\n", Members.Select(x => MemberDeclaration(x, model)))}}

            public static explicit operator {{Latest.Name}}Wrapper(SyntaxNode node)
            {
                if (node is null)
                {
                    return default;
                }

                if (!IsInstance(node))
                {
                    throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                }

                return new {{Latest.Name}}Wrapper(({{CompiletimeTypeSnippet()}})node);
            }

            public static implicit operator {{CompiletimeTypeSnippet()}}({{Latest.Name}}Wrapper wrapper) =>
                wrapper.node;

            public static bool IsInstance(SyntaxNode node) =>
                node is not null && LightupHelpers.CanWrapNode(node, WrappedType);
        }
        """;

    private string MemberAccessorInitialization(MemberInfo member, StrategyModel model)
    {
        if (member is PropertyInfo pi)
        {
            return $"""
                        {member.Name}Accessor = LightupHelpers.CreateSyntaxPropertyAccessor<{BaseType.Name}, {model[pi.PropertyType].CompiletimeTypeSnippet()}>(WrappedType, nameof({member.Name}));
                """;
        }
        return null;
    }

    private string MemberDeclaration(MemberDescriptor member, StrategyModel model) =>
        member switch
        {
            { IsPassthrough: true, Member: PropertyInfo pi } => $"""
                    public {pi.PropertyType.Name} {member.Member.Name} => this.node.{member.Member.Name};
                """,
            { IsPassthrough: false, Member: PropertyInfo pi } when model[pi.PropertyType] is var propertyTypeStrategy => $"""
                    private static readonly Func<{BaseType.Name}, {propertyTypeStrategy.CompiletimeTypeSnippet()}> {member.Member.Name}Accessor;
                    public {propertyTypeStrategy.ReturnTypeSnippet()} {member.Member.Name} => {model.ToConversionSnippet(pi.PropertyType, $"{member.Member.Name}Accessor(this.node)")};
                """,
            _ => null,
        };
}
