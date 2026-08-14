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

namespace SonarAnalyzer.ShimLayer.Generator.Strategies;

public sealed class ExtendStrategy : Strategy
{
    public IReadOnlyList<MemberInfo> Members { get; }

    public ExtendStrategy(Type latest, MemberDescriptor[] members) : base(latest) =>
        Members = members.Where(x => !x.IsPassthrough).Select(x => x.Member).ToArray();

    public override string ReturnTypeSnippet() =>
        Latest.Name;

    public override string ToConversionSnippet(string from) =>
        from;

    protected override string GenerateCore(StrategyModel model) =>
        Members.Select(x => GenerateMemberAccessor(x, model)).Where(x => x is not null).ToArray() is { Length: > 0 } accessors
            ? $$"""
                {{Preamble($"using {Latest.Namespace};")}}
                public static partial class {{Latest.Name}}ShimExtensions
                {
                    private static readonly Type WrappedType = typeof({{CompiletimeTypeSnippet()}});

                {{JoinLines(accessors)}}

                    extension({{CompiletimeTypeSnippet()}} wrappedInstance)
                    {
                {{JoinLines(Members.Select(x => GenerateMemberExtension(x, model)))}}
                    }
                }
                """
            : null;

    private string GenerateMemberAccessor(MemberInfo member, StrategyModel model) =>
        member switch
        {
            PropertyInfo prop when model[prop.PropertyType] is { IsSupported: true } propertyTypeStrategy => $"""
                    private static readonly Func<{CompiletimeTypeSnippet()}, {propertyTypeStrategy.CompiletimeTypeSnippet()}> {prop.Name}Accessor = {propertyTypeStrategy.PropertyAccessorInitializerSnippet(CompiletimeTypeSnippet(), prop.Name)};
                """,
            _ => null,
        };

    private static string GenerateMemberExtension(MemberInfo member, StrategyModel model) =>
        member switch
        {
            PropertyInfo { GetMethod: not null } prop when model[prop.PropertyType] is { IsSupported: true } propertyTypeStrategy => $"""
                        public {propertyTypeStrategy.ReturnTypeSnippet()} {prop.Name} => {propertyTypeStrategy.ToConversionSnippet($"{prop.Name}Accessor(wrappedInstance)")};
                """,
            _ => null,
        };
}
