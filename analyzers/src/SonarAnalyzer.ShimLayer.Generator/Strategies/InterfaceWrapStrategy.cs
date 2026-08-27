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

public class InterfaceWrapStrategy : WrapStrategy
{
    protected override string BaseTypeSnippet => null;
    protected override string FromTypeName => "object";
    protected override string ConversionSnippet => null;

    public InterfaceWrapStrategy(Type latest, Type baseType, MemberDescriptor[] members) : base(latest, baseType, null, members) { }

    protected override string WrapperToWrapperConversions(StrategyModel model) =>
        WrapperToWrapperConversions(Latest.GetInterfaces().Where(x => model[x] is WrapStrategy));
}
