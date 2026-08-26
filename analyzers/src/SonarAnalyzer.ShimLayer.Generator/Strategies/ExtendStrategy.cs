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

public class ExtendStrategy : MemberStrategy
{
    public override string ReturnTypeSnippet => Latest.Name;

    public ExtendStrategy(Type latest, MemberDescriptor[] members) : base(latest, members) { }

    public override string ToConversionSnippet(string from) =>
        from;

    protected override string GenerateCore(StrategyModel model)
    {
        var wrap = WrapMembers(model);
        return wrap.Properties.Any() || wrap.Methods.Any()
            ? $$"""
                {{Preamble($"using {Latest.Namespace};")}}
                public static partial class {{Latest.Name}}ShimExtensions
                {
                    private static readonly Type WrappedType = typeof({{CompiletimeTypeSnippet}});

                {{JoinLines(wrap.Properties.Select(x => x.AccessorDeclaration()))}}

                {{JoinLines(wrap.Methods.Select(x => x.AccessorDeclaration()))}}

                    extension({{CompiletimeTypeSnippet}} wrappedInstance)
                    {
                {{JoinLines(wrap.Properties.Select(x => x.MemberDeclaration(8)))}}

                {{JoinLines(wrap.Methods.Select(x => x.MemberDeclaration(8)))}}

                {{AdditionalMembers(model)}}
                    }
                }
                """
            : null;
    }

    protected virtual string AdditionalMembers(StrategyModel model) => null;
}
