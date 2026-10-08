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

public class GenericTypeStrategy : Strategy
{
    private readonly Strategy[] typeArguments;
    private readonly string type;

    public override bool IsSupported => (Latest.Name == "Action`1" && typeArguments.All(x => x.IsSupported)) || typeArguments.All(x => x is not WrapStrategy && x.IsSupported);
    public override string TypeSnippet => type ?? throw new NotSupportedException();
    public override string CompiletimeTypeSnippet => type ?? throw new NotSupportedException();

    public GenericTypeStrategy(Type latest, Strategy[] typeArguments) : base(latest)
    {
        this.typeArguments = typeArguments;
        type = IsSupported ? latest.Name.Split('`')[0] + "<" + typeArguments.JoinStr(", ", x => x.ReturnTypeSnippet) + ">" : null;
    }

    protected override string GenerateCore(StrategyModel model) => null;
}
