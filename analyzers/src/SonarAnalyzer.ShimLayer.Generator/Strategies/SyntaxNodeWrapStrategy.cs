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

            private static readonly Type WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
            private readonly {{CompiletimeTypeSnippet()}} wrappedInstance;

            private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} wrappedInstance) =>
                this.wrappedInstance = wrappedInstance;

            [Obsolete("Use WrappedInstance instead")]
            public {{CompiletimeTypeSnippet()}} Node => wrappedInstance;

            [Obsolete("Use WrappedInstance instead")]
            public {{CompiletimeTypeSnippet()}} SyntaxNode => wrappedInstance;

            public {{CompiletimeTypeSnippet()}} WrappedInstance => wrappedInstance;

        {{JoinLines(Members.Select(x => MemberDeclaration(x, model)))}}

            public static explicit operator {{Latest.Name}}Wrapper(SyntaxNode node) =>
                From(node);

            public static implicit operator {{CompiletimeTypeSnippet()}}({{Latest.Name}}Wrapper wrapper) =>
                wrapper.wrappedInstance;

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
}
