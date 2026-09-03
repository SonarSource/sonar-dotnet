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

public class StaticClassStrategy : MemberStrategy
{
    public override string ReturnTypeSnippet => throw new NotSupportedException();

    public StaticClassStrategy(Type latest, MemberDescriptor[] members) : base(latest, members) { }

    protected override string GenerateCore(StrategyModel model)
    {
        var wrap = WrapMembers(model);
        return wrap.Properties.Any() || wrap.Methods.Any()
            ? $$"""
                {{Preamble()}}
                public static class {{Latest.Name}}Ex
                {
                    private static readonly Type WrappedType = TypeRegister.LatestType("{{Latest.FullName}}");

                {{JoinLines(wrap.Properties.Select(x => x.AccessorDeclaration()))}}

                {{JoinLines(wrap.Methods.Select(x => x.AccessorDeclaration()))}}

                {{JoinLines(wrap.Properties.Select(x => x.MemberDeclaration(4)))}}

                {{JoinLines(wrap.Methods.Select(x => x.MemberDeclaration(4)))}}
                }
                """
            : null;
    }
}
