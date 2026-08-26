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

namespace SonarAnalyzer.ShimLayer.Generator.Snippets;

public sealed class MethodWrapSnippet : MethodSnippet
{
    public MethodWrapSnippet(Strategy strategy, MemberDescriptor member, Strategy returnType, StrategyModel model) : base(strategy, member, returnType, model) { }

    public override string AccessorDeclaration()
    {
        if (parameters.Any(x => x.IsOut))
        {
            var parametersSnippet = ((string[])[$"{strategy.CompiletimeTypeSnippet} sender", .. parameters.Select(SerializeParameter)]).JoinStr(", ");
            return $"""
                    private delegate {returnType.CompiletimeTypeSnippet} {accessorName}Delegate({parametersSnippet});
                    private static readonly {accessorName}Delegate {accessorName} = AccessorFactory.CreateMethod<{accessorName}Delegate>(WrappedType, "{member.Name}");
                """;
        }
        else
        {
            var types = parameters.Select(x => model[x.ParameterType].ReturnTypeSnippet).Prepend(strategy.CompiletimeTypeSnippet);
            string delegateName;
            if (returnType.Latest.FullName == typeof(void).FullName)
            {
                delegateName = "Action";
            }
            else
            {
                delegateName = "Func";
                types = types.Append(returnType.CompiletimeTypeSnippet);
            }
            var typesSnippet = types.JoinStr(", ");
            return $"""
                    private static readonly {delegateName}<{typesSnippet}> {accessorName} = AccessorFactory.CreateMethod<{delegateName}<{typesSnippet}>>(WrappedType, "{member.Name}");
                """;
        }
    }

    protected override string InvocationSnippet() =>
        $"""
        {returnType.ToConversionSnippet($"{accessorName}({parameters.Select(SerializeParameterArgument).Prepend("wrappedInstance").JoinStr(", ")})")}
        """;
}
