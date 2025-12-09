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
    public IReadOnlyList<MemberDescriptor> Members { get; }

    public SyntaxNodeStrategy(Type latest, IReadOnlyList<MemberDescriptor> members)
    {
        Latest = latest;
        Members = members;
    }

    public override string Generate(IReadOnlyDictionary<Type, Strategy> model) =>
        $$"""
        namespace SonarAnalyzer.ShimLayer;

        public readonly struct {{Latest.Name}}Wrapper
        {
            private readonly Microsoft.CodeAnalysis.SyntaxNode _node;
            public {{Latest.Name}}Wrapper(Microsoft.CodeAnalysis.SyntaxNode node)
            {
                _node = node;
            }
            public Microsoft.CodeAnalysis.SyntaxNode SyntaxNode => _node;

            {{string.Join(Environment.NewLine + "    " + Environment.NewLine, Members.Where(x => !x.IsPassthrough).Select(x => GenerateMemberWrapper(x.Member, model)))}}
            {{string.Join(Environment.NewLine + "    " + Environment.NewLine, Members.Where(x => x.IsPassthrough).Select(x => GeneratePassThrough(x.Member, model)))}}
        }
        """;

    private string GenerateMemberWrapper(MemberInfo member, IReadOnlyDictionary<Type, Strategy> model)
    {
        return $$"""public Wrap {{member.Name}} { get; set; }""";
    }

    private string GeneratePassThrough(MemberInfo member, IReadOnlyDictionary<Type, Strategy> model)
    {
        return $$"""public PassThrough {{member.Name}} { get; set; }""";
    }
}
