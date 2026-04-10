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

public class SkipStrategy : Strategy
{
    public override bool IsSupported => false;

    public SkipStrategy(Type latest) : base(latest) { }

    public override string ReturnTypeSnippet() =>
        throw new NotSupportedException();

    public override string ToConversionSnippet(string from) =>
        throw new NotSupportedException();

    public override string Generate(StrategyModel model) => null;
}
