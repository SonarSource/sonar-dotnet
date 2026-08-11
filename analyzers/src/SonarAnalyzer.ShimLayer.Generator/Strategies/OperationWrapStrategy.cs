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

public class OperationWrapStrategy : Strategy
{
    public IReadOnlyList<MemberDescriptor> Members { get; }

    public OperationWrapStrategy(Type latest, IReadOnlyList<MemberDescriptor> members) : base(latest) =>
        Members = members;

    public override string CompiletimeTypeSnippet() =>
        "IOperation";

    public override string ReturnTypeSnippet() =>
        $"{Latest.Name}Wrapper";

    public override string ToConversionSnippet(string from) =>
        $"{Latest.Name}Wrapper.FromOperation({from})";

    // ToDo: Remove FIXME class name suffix
    protected override string GenerateCore(StrategyModel model) =>
        $$"""
        {{Preamble()}}
        public readonly partial struct {{Latest.Name}}WrapperFIXME : IOperationWrapper
        {
            public const string WrappedTypeName = "{{Latest.FullName}}";
            private static readonly Type WrappedType;

            private readonly {{CompiletimeTypeSnippet()}} operation;

            static {{Latest.Name}}WrapperFIXME()
            {
                WrappedType = TypeRegister.LatestType(typeof({{Latest.Name}}WrapperFIXME));
            }

            private {{Latest.Name}}WrapperFIXME({{CompiletimeTypeSnippet()}} operation) =>
                this.operation = operation;

            public {{CompiletimeTypeSnippet()}} WrappedOperation => this.operation;

            public static {{Latest.Name}}WrapperFIXME FromOperation(IOperation operation)
            {
                if (operation is null)
                {
                    return default;
                }
                else if (IsInstance(operation))
                {
                    return new {{Latest.Name}}WrapperFIXME(operation);
                }
                else
                {
                    throw new InvalidCastException($"Cannot cast '{operation.GetType().FullName}' to '{WrappedTypeName}'");
                }
            }

            public static bool IsInstance(IOperation operation) =>
                operation is not null && LightupHelpers.CanWrapOperation(operation, WrappedType);
        }
        """;
}
