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

public abstract class WrapStrategy : Strategy
{
    protected abstract string BaseTypeSnippet { get; }
    [Obsolete("This should be removed once we remove the obsolete usages from the generated code")]
    protected abstract string ObsoletePropertiesSnippet { get; }
    protected abstract string ConversionSnippet { get; }
    protected abstract string WrapperToWrapperConversions(StrategyModel model);

    public Type BaseType { get; }
    public IReadOnlyList<MemberDescriptor> Members { get; }

    protected WrapStrategy(Type latest, Type baseType, IReadOnlyList<MemberDescriptor> members) : base(latest)
    {
        BaseType = baseType;
        Members = members;
    }

    public override string ReturnTypeSnippet() =>
        $"{Latest.Name}Wrapper";

    public override string ToConversionSnippet(string from) =>
        $"{Latest.Name}Wrapper.From({from})";

    public override string CompiletimeTypeSnippet() =>
        BaseType.Name;

    protected override string GenerateCore(StrategyModel model)
    {
        return $$"""
            {{Preamble()}}
            public readonly partial struct {{Latest.Name}}Wrapper : {{BaseTypeSnippet}}
            {
                public const string WrappedTypeName = "{{Latest.FullName}}";

                private static readonly Type WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
                private readonly {{CompiletimeTypeSnippet()}} wrappedInstance;

                private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} wrappedInstance) =>
                    this.wrappedInstance = wrappedInstance;

            {{ObsoletePropertiesSnippet}}

                public {{CompiletimeTypeSnippet()}} WrappedInstance => wrappedInstance;

            {{JoinLines(Members.Select(x => MemberDeclaration(x, model)))}}

            {{ConversionSnippet}}

            {{WrapperToWrapperConversions(model)}}
            }
            """;
    }

    protected string WrapperToWrapperConversions(IEnumerable<Type> baseTypes)
    {
        StringBuilder sb = null;
        foreach (var baseType in baseTypes.Select(x => x.Name))
        {
            sb ??= new StringBuilder();
            sb.AppendLine($"""
                    public static implicit operator {baseType}Wrapper({Latest.Name}Wrapper up) => {baseType}Wrapper.From(up.WrappedInstance);
                    public static explicit operator {Latest.Name}Wrapper({baseType}Wrapper down) => {Latest.Name}Wrapper.From(down.WrappedInstance);

                """);
        }
        return sb?.ToString();
    }

    protected string MemberDeclaration(MemberDescriptor member, StrategyModel model) =>
        member switch
        {
            { IsPassthrough: true, Member: PropertyInfo pi } when new PropertyPassthroughSnippet(this, pi, model[pi.PropertyType]) is var snippet => $"""
                {snippet.MemberDeclaration(4)}
                """,
            { IsPassthrough: false, Member: PropertyInfo pi } when model[pi.PropertyType] is { IsSupported: true } propertyTypeStrategy && new PropertyWrapSnippet(this, pi, propertyTypeStrategy) is var snippet => $"""
                {snippet.AccessorDeclaration()}
                {snippet.MemberDeclaration(4)}
                """,
            _ => null,
        };
}
