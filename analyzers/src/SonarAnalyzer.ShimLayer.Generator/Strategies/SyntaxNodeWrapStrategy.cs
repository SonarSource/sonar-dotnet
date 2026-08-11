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

public class SyntaxNodeWrapStrategy : WrapStrategy
{
    public SyntaxNodeWrapStrategy(Type latest, Type baseType, IReadOnlyList<MemberDescriptor> members) : base(latest, baseType, members) { }

    protected override string GenerateCore(StrategyModel model) =>
        $$"""
        {{Preamble()}}
        public readonly partial struct {{Latest.Name}}Wrapper : ISyntaxWrapper<{{CompiletimeTypeSnippet()}}>
        {
            public const string WrappedTypeName = "{{Latest.FullName}}";
            private static readonly Type WrappedType;

            private readonly {{CompiletimeTypeSnippet()}} node;

            static {{Latest.Name}}Wrapper()
            {
                WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
        {{JoinLines(Members.Where(x => !x.IsPassthrough).Select(x => MemberAccessorInitialization(x.Member, model)))}}
            }

            private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} node) =>
                this.node = node;

            [Obsolete("Use WrappedInstance instead")]
            public {{CompiletimeTypeSnippet()}} Node => this.node;

            [Obsolete("Use WrappedInstance instead")]
            public {{CompiletimeTypeSnippet()}} SyntaxNode => this.node;

            public {{CompiletimeTypeSnippet()}} WrappedInstance => this.node;

        {{JoinLines(Members.Select(x => MemberDeclaration(x, model)))}}

            public static explicit operator {{Latest.Name}}Wrapper(SyntaxNode node) =>
                From(node);

            public static implicit operator {{CompiletimeTypeSnippet()}}({{Latest.Name}}Wrapper wrapper) =>
                wrapper.node;

            public static {{Latest.Name}}Wrapper From(SyntaxNode node)
            {
                if (node is null)
                {
                    return default;
                }
                else if (IsInstance(node))
                {
                    return new {{Latest.Name}}Wrapper(({{CompiletimeTypeSnippet()}})node);
                }
                else
                {
                    throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{WrappedTypeName}'");
                }
            }

            public static bool IsInstance(SyntaxNode node) =>
                node is not null && LightupHelpers.CanWrapNode(node, WrappedType);

        {{WrapperToWrapperConversions(model)}}
        }
        """;

    private string WrapperToWrapperConversions(StrategyModel model)
    {
        return WrapperToWrapperConversions(WrappedBaseTypes());

        IEnumerable<Type> WrappedBaseTypes()
        {
            var baseType = Latest.BaseType;
            while (baseType is not null && model[baseType] is SyntaxNodeWrapStrategy) // BaseType is also wrapped
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }

    private string MemberAccessorInitialization(MemberInfo member, StrategyModel model) =>
        member is PropertyInfo property && model[property.PropertyType] is { IsSupported: true } propertyTypeStrategy
            ? $"""
                        {member.Name}Accessor = {propertyTypeStrategy.PropertyAccessorInitializerSnippet(CompiletimeTypeSnippet(), member.Name)};
                """
            : null;

    private string MemberDeclaration(MemberDescriptor member, StrategyModel model)
    {
        var attributes = SerializeAttributes(member.Member.GetCustomAttributesData(), 4);
        return member switch
        {
            { IsPassthrough: true, Member: PropertyInfo pi } => $"""
                    {attributes}public {model[pi.PropertyType].CompiletimeTypeSnippet()} {member.Member.Name} => this.node.{member.Member.Name};
                """,
            { IsPassthrough: false, Member: PropertyInfo pi } when model[pi.PropertyType] is { IsSupported: true } propertyTypeStrategy => $"""
                    private static readonly Func<{BaseType.Name}, {propertyTypeStrategy.CompiletimeTypeSnippet()}> {member.Member.Name}Accessor;
                    {attributes}public {propertyTypeStrategy.ReturnTypeSnippet()} {member.Member.Name} => {propertyTypeStrategy.ToConversionSnippet($"{member.Member.Name}Accessor(this.node)")};
                """,
            _ => null,
        };
    }
}
