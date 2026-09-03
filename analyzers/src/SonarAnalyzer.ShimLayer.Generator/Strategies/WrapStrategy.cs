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
    protected abstract string FromTypeName { get; }
    protected abstract string ConversionSnippet { get; }

    public Type BaseType { get; }
    public Type FallbackBaseType { get; }
    public override string TypeSnippet => $"{Latest.Name}Wrapper";
    public override string ReturnTypeSnippet => $"{TypeSnippet}{NullableSnippet}";
    public override string CompiletimeTypeSnippet => BaseType.Name;
    protected virtual string BaseTypeSnippet => $"IWrapper, IEquatable<{TypeSnippet}>";
    protected virtual bool IsNullable => false;
    private string NullableSnippet => IsNullable ? "?" : null;

    protected WrapStrategy(Type latest, Type baseType, Type fallbackBaseType, MemberDescriptor[] members) : base(latest, members)
    {
        BaseType = baseType;
        FallbackBaseType = fallbackBaseType;
    }

    protected override string GenerateCore(StrategyModel model)
    {
        var passthrough = PassthroughMembers(model);
        var wrap = WrapMembers(model);
        return $$"""
            {{Preamble()}}
            public readonly struct {{TypeSnippet}} : {{BaseTypeSnippet}}
            {
                private static readonly Type WrappedType = TypeRegister.LatestType("{{Latest.FullName}}"{{FallbackBaseTypeSnippet()}});
                private static readonly ConcurrentDictionary<Type, bool> CanWrapCache = new();
                private readonly {{CompiletimeTypeSnippet}} wrappedInstance;

            {{JoinLines(wrap.Properties.Select(x => x.AccessorDeclaration()))}}

            {{JoinLines(wrap.Methods.Select(x => x.AccessorDeclaration()))}}

                private {{TypeSnippet}}({{CompiletimeTypeSnippet}} wrappedInstance) =>
                    this.wrappedInstance = wrappedInstance;

                public {{CompiletimeTypeSnippet}} WrappedInstance => wrappedInstance;

                object IWrapper.WrappedInstance => wrappedInstance;

                public override int GetHashCode() =>
                    wrappedInstance?.GetHashCode() ?? 0;

                public override bool Equals(object obj) =>
                    (obj is IWrapper wrapper && Equals(wrappedInstance, wrapper.WrappedInstance))
                    || Equals(wrappedInstance, obj);

                public bool Equals({{TypeSnippet}} other) =>
                    Equals(wrappedInstance, other.wrappedInstance);

                public static bool operator ==({{TypeSnippet}} left, {{TypeSnippet}} right) =>
                    Equals(left.wrappedInstance, right.wrappedInstance);

                public static bool operator !=({{TypeSnippet}} left, {{TypeSnippet}} right) =>
                    !Equals(left.wrappedInstance, right.wrappedInstance);

            {{JoinLines(passthrough.Properties.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(wrap.Properties.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(passthrough.Methods.Select(x => x.MemberDeclaration(4)))}}

            {{JoinLines(wrap.Methods.Select(x => x.MemberDeclaration(4)))}}

            {{ConversionSnippet}}

                public static {{ReturnTypeSnippet}} From({{FromTypeName}} instance)
                {
                    if (instance is null)
                    {
                        return default;
                    }
                    else if (IsInstance(instance))
                    {
                        return new {{TypeSnippet}}(({{CompiletimeTypeSnippet}})instance);
                    }
                    else
                    {
                        throw new InvalidCastException($"Cannot cast '{instance.GetType().FullName}' to '{{Latest.FullName}}'");
                    }
                }

                public static bool IsInstance({{FromTypeName}} instance) =>
                    WrappedType.CanWrap(CanWrapCache, instance);

            {{FallbackBaseTypeConversionSnippet()}}

            {{WrapperToWrapperConversions(model)}}
            }
            """;
    }

    protected virtual string WrapperToWrapperConversions(StrategyModel model)
    {
        return WrapperToWrapperConversions(WrappedBaseTypes());

        IEnumerable<Type> WrappedBaseTypes()
        {
            var baseType = Latest.BaseType;
            while (baseType is not null && model[baseType] is WrapStrategy) // BaseType is also wrapped
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }

    protected string WrapperToWrapperConversions(IEnumerable<Type> baseTypes)
    {
        StringBuilder sb = null;
        foreach (var baseType in baseTypes.Select(x => x.Name))
        {
            sb ??= new StringBuilder();
            sb.AppendLine($"""
                    public static implicit operator {baseType}Wrapper({TypeSnippet} up) => {baseType}Wrapper.From(up.WrappedInstance);
                    public static explicit operator {TypeSnippet}({baseType}Wrapper down) => {TypeSnippet}.From(down.WrappedInstance);

                """);
        }
        return sb?.ToString();
    }

    private string FallbackBaseTypeSnippet() =>
        FallbackBaseType is null
            ? null
            : $"""
                , "{FallbackBaseType.FullName}"
                """;

    private string FallbackBaseTypeConversionSnippet() =>
        FallbackBaseType is null
            ? null
            : $"""    public static implicit operator {TypeSnippet}({FallbackBaseType.Name} instance) => new(instance);""";
}
