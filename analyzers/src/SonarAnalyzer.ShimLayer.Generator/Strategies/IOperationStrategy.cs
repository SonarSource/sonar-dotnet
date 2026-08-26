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

public sealed class IOperationStrategy : ExtendStrategy
{
    public IOperationStrategy(Type latest, MemberDescriptor[] members) : base(latest, members)
    {
        if (latest.FullName != typeof(IOperation).FullName)
        {
            throw new ArgumentException($"{nameof(IOperationStrategy)} should be used only with {nameof(IOperation)} itself.");
        }
    }

    protected override string AdditionalMembers(StrategyModel model)
    {
        var operations = model.OfType<OperationWrapStrategy>().OrderBy(x => x.ReturnTypeSnippet).ToArray();
        return $"""
            {JoinLines(operations.Select(AsOperationMethod))}

            {JoinLines(operations.Select(ToOperationMethod))}
            """;
    }

    private static string AsOperationMethod(OperationWrapStrategy strategy) =>
        $"""
                public {strategy.ReturnTypeSnippet}? As{ShortName(strategy)} => {strategy.ReturnTypeSnippet}.FromOrDefault(wrappedInstance);
        """;

    private static string ToOperationMethod(OperationWrapStrategy strategy) =>
        $"""
                public {strategy.ReturnTypeSnippet} To{ShortName(strategy)}() => {strategy.ReturnTypeSnippet}.From(wrappedInstance);
        """;

    private static string ShortName(OperationWrapStrategy strategy) =>
        strategy.Latest.Name.Substring(1, strategy.Latest.Name.Length - nameof(IOperation).Length);
}
