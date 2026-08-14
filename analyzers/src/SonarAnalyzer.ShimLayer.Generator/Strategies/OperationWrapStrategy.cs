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

    protected override string ObsoletePropertiesSnippet => $"""
            [Obsolete("Use WrappedInstance instead")]
            public {CompiletimeTypeSnippet()} WrappedOperation => wrappedInstance;
        """;

    protected override string ConversionSnippet => $$"""
            [Obsolete("Use From instead")]
            public static {{Latest.Name}}Wrapper FromOperation(IOperation operation) =>
                From(operation);

            public static {{Latest.Name}}Wrapper From(IOperation operation)
            {
                if (operation is null)
                {
                    return default;
                }
                else if (IsInstance(operation))
                {
                    return new {{Latest.Name}}Wrapper(operation);
                }
                else
                {
                    throw new InvalidCastException($"Cannot cast '{operation.GetType().FullName}' to '{WrappedTypeName}'");
                }
            }

            public static bool IsInstance(IOperation operation) =>
                operation is not null && LightupHelpers.CanWrapOperation(operation, WrappedType);
        """;

    public OperationWrapStrategy(Type latest, IReadOnlyList<MemberDescriptor> members) : base(latest, typeof(IOperation), members) { }

    protected override string WrapperToWrapperConversions(StrategyModel model) =>
        WrapperToWrapperConversions(Latest.GetInterfaces().Where(x => model[x] is OperationWrapStrategy));
}
