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

public class OperationWrapStrategy : WrapStrategy
{
    protected override string BaseTypeSnippet => "IOperationWrapper";
    protected override string FromTypeName => "IOperation";

    protected override string ObsoletePropertiesSnippet => null;

    protected override string ConversionSnippet => $"""
            public static {ReturnTypeSnippet()}? FromOrDefault(IOperation instance) =>
                IsInstance(instance) ? From(instance) : null;

            [Obsolete("Use From instead")]
            public static {ReturnTypeSnippet()} FromOperation(IOperation instance) =>
                From(instance);
        """;

    public OperationWrapStrategy(Type latest, MemberDescriptor[] members) : base(latest, typeof(IOperation), null, members) { }

    protected override string WrapperToWrapperConversions(StrategyModel model) =>
        WrapperToWrapperConversions(Latest.GetInterfaces().Where(x => model[x] is OperationWrapStrategy));
}
