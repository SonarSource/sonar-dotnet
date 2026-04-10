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

public class NoChangeStrategy : Strategy
{
    private readonly string type;

    public NoChangeStrategy(Type latest) : base(latest) =>
        type = latest.IsGenericType
            ? latest.Name.Replace("`1", null) + "<" + string.Join(", ", latest.GetGenericArguments().Select(x => x.Name)) + ">"
            : latest.Name;

    public override string Generate(StrategyModel model) => null;

    public override string ReturnTypeSnippet() =>
        type;

    public override string CompiletimeTypeSnippet() =>
        type;

    public override string ToConversionSnippet(string from) =>
        $"({type}){from}";
}
