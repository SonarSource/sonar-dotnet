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
        var passthroughProperties = Members
            .Select(x => x.IsPassthrough && x.Member is PropertyInfo pi ? new PropertyPassthroughSnippet(this, x, model[pi.PropertyType]) : null)
            .Where(x => x is not null)
            .ToArray();
        var wrapProperties = Members
            .Select(x => !x.IsPassthrough && x.Member is PropertyInfo pi && model[pi.PropertyType] is { IsSupported: true } returnType ? new PropertyWrapSnippet(this, x, returnType) : null)
            .Where(x => x is not null)
            .ToArray();

        return $$"""
            {{Preamble()}}
            public readonly partial struct {{Latest.Name}}Wrapper : {{BaseTypeSnippet}}
            {
                public const string WrappedTypeName = "{{Latest.FullName}}";

                private static readonly Type WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
                private readonly {{CompiletimeTypeSnippet()}} wrappedInstance;

            {{JoinLines(wrapProperties.Select(x => x.AccessorDeclaration()))}}

                private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} wrappedInstance) =>
                    this.wrappedInstance = wrappedInstance;

            {{ObsoletePropertiesSnippet}}

                public {{CompiletimeTypeSnippet()}} WrappedInstance => wrappedInstance;

            {{JoinLines(passthroughProperties.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(wrapProperties.Select(x => x.MemberDeclaration(4)))}}

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
}
