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
    public OperationWrapStrategy(Type latest, IReadOnlyList<MemberDescriptor> members) : base(latest, typeof(IOperation), members) { }

    // ToDo: Change internal to public
    protected override string GenerateCore(StrategyModel model) =>
        $$"""
        {{Preamble()}}
        internal readonly partial struct {{Latest.Name}}Wrapper : IOperationWrapper
        {
            public const string WrappedTypeName = "{{Latest.FullName}}";
            private static readonly Type WrappedType;

            private readonly {{CompiletimeTypeSnippet()}} operation;

            static {{Latest.Name}}Wrapper()
            {
                WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}Wrapper));
            }

            private {{Latest.Name}}Wrapper({{CompiletimeTypeSnippet()}} operation) =>
                this.operation = operation;

            [Obsolete("Use WrappedInstance instead")]
            public {{CompiletimeTypeSnippet()}} WrappedOperation => this.operation;

            public {{CompiletimeTypeSnippet()}} WrappedInstance => this.operation;

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

        {{WrapperToWrapperConversions(model)}}
        }
        """;

    private string WrapperToWrapperConversions(StrategyModel model) =>
        WrapperToWrapperConversions(Latest.GetInterfaces().Where(x => model[x] is OperationWrapStrategy));
}
