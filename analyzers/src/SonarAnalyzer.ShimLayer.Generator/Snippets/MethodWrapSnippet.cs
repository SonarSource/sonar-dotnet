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
        var createMethodName = member.IsStatic ? "CreateStaticMethod" : "CreateMethod";
        if (parameters.Any(x => x.IsOut))
        {
            var parameterSnippets = member.IsStatic
                ? parameters.Select(SerializeParameter)
                : parameters.Select(SerializeParameter).Prepend($"{strategy.CompiletimeTypeSnippet} sender");
            return $"""
                    private delegate {returnType.ReturnTypeSnippet} {accessorName}Delegate({parameterSnippets.JoinStr(", ")});
                    private static readonly {accessorName}Delegate {accessorName} = AccessorFactory.{createMethodName}<{accessorName}Delegate>(WrappedType, "{member.Name}");
                """;
        }
        else
        {
            string delegateName;
            var types = new List<string>(parameters.Select(x => model[x.ParameterType].ReturnTypeSnippet));
            if (!member.IsStatic)
            {
                types.Insert(0, strategy.CompiletimeTypeSnippet);
            }
            if (returnType.Latest.FullName == typeof(void).FullName)
            {
                delegateName = "Action";
            }
            else
            {
                delegateName = "Func";
                types.Add(returnType.ReturnTypeSnippet);
            }
            var typesSnippet = types.Any() ? $"<{types.JoinStr(", ")}>" : null;
            return $"""
                    private static readonly {delegateName}{typesSnippet} {accessorName} = AccessorFactory.{createMethodName}<{delegateName}{typesSnippet}>(WrappedType, "{member.Name}");
                """;
        }
    }

    protected override string InvocationSnippet()
    {
        var parameterSnippets = member.IsStatic
            ? parameters.Select(SerializeParameterArgument)
            : parameters.Select(SerializeParameterArgument).Prepend("wrappedInstance");
        return $"""
            {accessorName}({parameterSnippets.JoinStr(", ")})
            """;
    }
}
