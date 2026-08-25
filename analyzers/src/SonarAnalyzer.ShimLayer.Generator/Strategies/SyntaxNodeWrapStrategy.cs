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
    protected override string BaseTypeSnippet => null;
    protected override string FromTypeName => "SyntaxNode";

    protected override string ObsoletePropertiesSnippet => $"""
            [Obsolete("Use WrappedInstance instead")]
            public {CompiletimeTypeSnippet()} Node => wrappedInstance;

            [Obsolete("Use WrappedInstance instead")]
            public {CompiletimeTypeSnippet()} SyntaxNode => wrappedInstance;
        """;

    protected override string ConversionSnippet => $"""
            public static explicit operator {Latest.Name}Wrapper(SyntaxNode instance) =>
                From(instance);

            public static implicit operator {CompiletimeTypeSnippet()}({Latest.Name}Wrapper wrapper) =>
                wrapper.wrappedInstance;
        """;

    public SyntaxNodeWrapStrategy(Type latest, Type baseType, MemberDescriptor[] members) : base(latest, baseType, members) { }

    protected override string WrapperToWrapperConversions(StrategyModel model)
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
