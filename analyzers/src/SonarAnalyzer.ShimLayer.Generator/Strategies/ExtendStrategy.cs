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
    public IReadOnlyList<MemberDescriptor> Members { get; }

    public ExtendStrategy(Type latest, MemberDescriptor[] members) : base(latest) =>
        Members = members.Where(x => !x.IsPassthrough).ToArray();

    public override string ReturnTypeSnippet() =>
        Latest.Name;

    public override string ToConversionSnippet(string from) =>
        from;

    protected override string GenerateCore(StrategyModel model)
    {
        var properties = Members
            .Select(x => x.Member is PropertyInfo pi && model[pi.PropertyType] is { IsSupported: true } returnType ? new PropertyWrapSnippet(this, x, returnType) : null)
            .Where(x => x is not null)
            .ToArray();
        return properties.Any()
            ? $$"""
                {{Preamble($"using {Latest.Namespace};")}}
                public static partial class {{Latest.Name}}ShimExtensions
                {
                    private static readonly Type WrappedType = typeof({{CompiletimeTypeSnippet()}});

                {{JoinLines(properties.Select(x => x.AccessorDeclaration()))}}

                    extension({{CompiletimeTypeSnippet()}} wrappedInstance)
                    {
                {{JoinLines(properties.Select(x => x.MemberDeclaration(8)))}}
                    }
                }
                """
            : null;
    }
}
