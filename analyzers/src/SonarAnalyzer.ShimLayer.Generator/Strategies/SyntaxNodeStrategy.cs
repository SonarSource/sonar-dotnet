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

public sealed class SyntaxNodeStrategy : ExtendStrategy
{
    // The As/To gates and the SyntaxNodeAs/SyntaxNodeTo structs must exist even when SyntaxNode itself has no new wrapped members, because every XxxSyntaxWrapper unconditionally forwards to them.
    protected override bool AlwaysGenerate => true;

    public SyntaxNodeStrategy(Type latest, MemberDescriptor[] members) : base(latest, members)
    {
        if (latest.FullName != typeof(SyntaxNode).FullName)
        {
            throw new ArgumentException($"{nameof(SyntaxNodeStrategy)} should be used only with {nameof(SyntaxNode)} itself.");
        }
    }

    protected override string AdditionalMembers(StrategyModel model) =>
        """
                public SyntaxNodeAs As => new(wrappedInstance);
                public SyntaxNodeTo To => new(wrappedInstance);
        """;

    protected override string AdditionalTopLevelTypes(StrategyModel model)
    {
        var nodes = model.OfType<SyntaxNodeWrapStrategy>().OrderBy(x => x.ReturnTypeSnippet).ToArray();
        return $$"""
            public readonly struct SyntaxNodeAs
            {
                private readonly SyntaxNode node;
                internal SyntaxNodeAs(SyntaxNode node) => this.node = node;

            {{JoinLines(nodes.Select(AsProperty))}}
            }

            public readonly struct SyntaxNodeTo
            {
                private readonly SyntaxNode node;
                internal SyntaxNodeTo(SyntaxNode node) => this.node = node;

            {{JoinLines(nodes.Select(ToProperty))}}
            }
            """;
    }

    private static string AsProperty(SyntaxNodeWrapStrategy strategy) =>
        $"""
            public {strategy.ReturnTypeSnippet}? {ShortName(strategy)} => {strategy.ReturnTypeSnippet}.FromOrDefault(node);
        """;

    // Throwing, unlike AsProperty, but still a property - only the null-vs-throw behavior differs between As and To, not the call shape.
    private static string ToProperty(SyntaxNodeWrapStrategy strategy) =>
        $"""
            public {strategy.ReturnTypeSnippet} {ShortName(strategy)} => {strategy.ReturnTypeSnippet}.From(node);
        """;

    private static string ShortName(SyntaxNodeWrapStrategy strategy) =>
        strategy.Latest.Name.EndsWith("Syntax", StringComparison.Ordinal)
            ? strategy.Latest.Name.Substring(0, strategy.Latest.Name.Length - "Syntax".Length)
            : strategy.Latest.Name;
}
