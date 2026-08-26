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

public class PrimitiveStrategy : Strategy
{
    private readonly string name;

    public override string ReturnTypeSnippet => name;
    public override string CompiletimeTypeSnippet => name;

    public PrimitiveStrategy(Type latest, string name) : base(latest) =>
        this.name = name;

    public override string ToConversionSnippet(string from) =>
        name == "void" ? from : $"({name}){from}";

    protected override string GenerateCore(StrategyModel model) => null;
}
