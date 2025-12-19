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

public sealed class SyntaxNodeExtendStrategy : Strategy
{
    public IReadOnlyList<MemberInfo> Members { get; }

    public SyntaxNodeExtendStrategy(Type latest, MemberDescriptor[] members) : base(latest) =>
        Members = members.Where(x => !x.IsPassthrough).Select(x => x.Member).ToArray();

    public override string Generate(StrategyModel model) =>
        Members.Select(x => GenerateMemberAccessor(x, model)).Where(x => x is not null).ToArray() is { Length: > 0 } accessors
            ? $$"""
                {{Preamble($"using {Latest.Namespace};")}}
                public static partial class {{Latest.Name}}ShimExtensions
                {
                    private static readonly Type WrappedType = typeof({{CompiletimeTypeSnippet()}});

                {{JoinLines(accessors)}}

                    extension({{CompiletimeTypeSnippet()}} @this)
                    {
                {{JoinLines(Members.Select(x => GenerateMemberExtension(x, model)))}}
                    }
                }
                """
            : null;

    public override string ReturnTypeSnippet() =>
        Latest.Name;

    public override string ToConversionSnippet(string from) => from;

    private string GenerateMemberAccessor(MemberInfo member, StrategyModel model) =>
        member switch
        {
            PropertyInfo prop when model[prop.PropertyType] is var propertyTypeStrategy => $"""
                    private static readonly Func<{CompiletimeTypeSnippet()}, {propertyTypeStrategy.CompiletimeTypeSnippet()}> {prop.Name}Accessor = {propertyTypeStrategy.PropertyAccessorInitializerSnippet(CompiletimeTypeSnippet(), prop.Name)};

                """,
            _ => null,
        };

    private static string GenerateMemberExtension(MemberInfo member, StrategyModel model) =>
        member switch
        {
            PropertyInfo { GetMethod: not null } prop when model[prop.PropertyType] is var propertyTypeStrategy => $"""
                        public {propertyTypeStrategy.ReturnTypeSnippet()} {prop.Name} => {propertyTypeStrategy.ToConversionSnippet($"{prop.Name}Accessor(@this)")};
                """,
            _ => null,
        };
}
