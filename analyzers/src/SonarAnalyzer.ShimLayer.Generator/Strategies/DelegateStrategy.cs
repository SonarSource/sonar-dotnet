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

public class DelegateStrategy : Strategy
{
    private readonly MethodInfo invoke;

    public override string TypeSnippet => $"{Latest.Name}Wrapper";

    public DelegateStrategy(Type latest) : base(latest) =>
        invoke = latest.GetMethod(nameof(Action.Invoke));

    protected override string GenerateCore(StrategyModel model) =>
        $"""
        {Preamble()}
        public delegate {model[invoke.ReturnType].ReturnTypeSnippet} {TypeSnippet}({invoke.GetParameters().JoinStr(", ", x => SerializeParameter(model, x))});
        """;

    private static string SerializeParameter(StrategyModel model, ParameterInfo info)
    {
        var type = info.ParameterType.IsByRef ? info.ParameterType.GetElementType() : info.ParameterType;
        if (info.ParameterType.IsByRef && model[type] is WrapStrategy)
        {
            throw new NotSupportedException($"{info.Member.DeclaringType.FullName}.{info.Member.Name}: by-ref parameter '{info.Name}' of wrapped type {type.Name} is not supported.");
        }
        return $"{Prefix()}{model[type].TypeSnippet} {Snippet.SerializeParameterName(info)}";

        string Prefix()
        {
            if (info.IsOut)
            {
                return "out ";
            }
            else if (info.IsIn)
            {
                return "in ";
            }
            else if (info.ParameterType.IsByRef)
            {
                return "ref ";
            }
            else if (info.GetCustomAttributesData().Any(x => x.AttributeType.Name == nameof(ParamArrayAttribute)))
            {
                return "params ";
            }
            else
            {
                return null;
            }
        }
    }
}
