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
    public Type Latest { get; }
    public Type BaseType { get; }
    public IReadOnlyList<MemberDescriptor> Members { get; }

    public SyntaxNodeStrategy(Type latest, Type baseType, IReadOnlyList<MemberDescriptor> members)
    {
        Latest = latest;
        BaseType = baseType;
        Members = members;
    }

    public override string Generate(IReadOnlyDictionary<Type, Strategy> model) =>
        $$"""
        using System;
        using System.Collections.Immutable;
        using Microsoft.CodeAnalysis;
        using Microsoft.CodeAnalysis.CSharp;
        using Microsoft.CodeAnalysis.CSharp.Syntax;

        namespace SonarAnalyzer.ShimLayer;

        public readonly partial struct {{Latest.Name}}Wrapper: ISyntaxWrapper<{{BaseType.Name}}>
        {
            public const string WrappedTypeName = "{{Latest.FullName}}";
            private static readonly Type WrappedType;

            private readonly {{BaseType.Name}} node;

            static {{Latest.Name}}Wrapper()
            {
                WrappedType = SyntaxWrapperHelper.GetWrappedType(typeof({{Latest.Name}}Wrapper));
        {{string.Join("\n", Members.Where(x => !x.IsPassthrough).Select(x => MemberAccessorInitialization(x.Member, model)))}}
            }

            private {{Latest.Name}}Wrapper({{BaseType.Name}} node) =>
                this.node = node;

            [Obsolete]
            public {{BaseType.Name}} SyntaxNode => this.node;

        {{string.Join("\n", Members.Select(x => MemberDeclaration(x, model)))}}
        }
        """;

    private string MemberAccessorInitialization(MemberInfo member, IReadOnlyDictionary<Type, Strategy> model)
    {
        if (member is PropertyInfo pi)
        {
            return $"""
                        {member.Name}Accessor = LightupHelpers.CreateSyntaxPropertyAccessor<{BaseType.Name}, {pi.PropertyType.Name}>(WrappedType, nameof({member.Name}));
                """;
        }
        return null;
    }

    private string MemberDeclaration(MemberDescriptor member, IReadOnlyDictionary<Type, Strategy> model) =>
        member switch
        {
            { IsPassthrough: true, Member: PropertyInfo pi } => $"""
                    public {pi.PropertyType.Name} {member.Member.Name} => this.node.{member.Member.Name};
                """,
            { IsPassthrough: false, Member: PropertyInfo pi } => $"""
                    private static readonly Func<{BaseType.Name}, {pi.PropertyType.Name}> {member.Member.Name}Accessor;
                    public {pi.PropertyType.Name} {member.Member.Name} => {member.Member.Name}Accessor(this.node);
                """,
            _ => null,
        };
}
