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

public abstract class WrapStrategy : MemberStrategy
{
    protected abstract string BaseTypeSnippet { get; }
    protected abstract string FromTypeName { get; }
    [Obsolete("This should be removed once we remove the obsolete usages from the generated code")]
    protected abstract string ObsoletePropertiesSnippet { get; }
    protected abstract string ConversionSnippet { get; }
    protected abstract string WrapperToWrapperConversions(StrategyModel model);

    public Type BaseType { get; }
    public Type FallbackBaseType { get; }

    protected WrapStrategy(Type latest, Type baseType, Type fallbackBaseType, MemberDescriptor[] members) : base(latest, members)
    {
        BaseType = baseType;
        FallbackBaseType = fallbackBaseType;
    }

    public override string ReturnTypeSnippet() =>
        $"{Latest.Name}Wrapper";

    public override string ToConversionSnippet(string from) =>
        $"{Latest.Name}Wrapper.From({from})";

    public override string CompiletimeTypeSnippet() =>
        BaseType.Name;

    protected override string GenerateCore(StrategyModel model)
    {
        var passthrough = PassthroughMembers(model);
        var wrap = WrapMembers(model);
        return $$"""
            {{Preamble()}}
            public readonly struct {{Latest.Name}}Wrapper{{(BaseTypeSnippet is null ? null : $" : {BaseTypeSnippet}")}}
            {
                public const string WrappedTypeName = "{{Latest.FullName}}";
            {{FallbackBaseTypeSnippet()}}

                private static readonly Type WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
                private static readonly ConcurrentDictionary<Type, bool> CanWrapCache = new();
                private readonly {{CompiletimeTypeSnippet()}} wrappedInstance;

            {{JoinLines(wrap.Properties.Select(x => x.AccessorDeclaration()))}}

            {{JoinLines(wrap.Methods.Select(x => x.AccessorDeclaration()))}}

                private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} wrappedInstance) =>
                    this.wrappedInstance = wrappedInstance;

            {{ObsoletePropertiesSnippet}}

                public {{CompiletimeTypeSnippet()}} WrappedInstance => wrappedInstance;

            {{JoinLines(passthrough.Properties.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(wrap.Properties.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(passthrough.Methods.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(wrap.Methods.Select(x => x.MemberDeclaration(4)))}}

            {{ConversionSnippet}}

                public static {{Latest.Name}}Wrapper From({{FromTypeName}} instance)
                {
                    if (instance is null)
                    {
                        return default;
                    }
                    else if (IsInstance(instance))
                    {
                        return new {{Latest.Name}}Wrapper(({{CompiletimeTypeSnippet()}})instance);
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot cast '{instance.GetType().FullName}' to '{WrappedTypeName}'");
                    }
                }

                public static bool IsInstance({{FromTypeName}} instance) =>
                    WrappedType.CanWrap(CanWrapCache, instance);

            {{FallbackBaseTypeConversionSnippet()}}

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

    private string FallbackBaseTypeSnippet() =>
        FallbackBaseType is null
            ? null
            : $"""    public const string FallbackWrappedTypeName = "{FallbackBaseType.FullName}";""";

    private string FallbackBaseTypeConversionSnippet() =>
        FallbackBaseType is null
            ? null
            : $"""    public static implicit operator {Latest.Name}Wrapper({FallbackBaseType.Name} instance) => new(instance);""";
}
