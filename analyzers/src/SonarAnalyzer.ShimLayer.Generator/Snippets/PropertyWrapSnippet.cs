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

public sealed class PropertyWrapSnippet : Snippet<PropertyInfo>
{
    public PropertyWrapSnippet(Strategy strategy, MemberDescriptor member, Strategy returnType) : base(strategy, member, returnType) { }

    public override string AccessorDeclaration() =>
        $"""
            private static readonly Func<{strategy.CompiletimeTypeSnippet()}, {returnType.CompiletimeTypeSnippet()}> {accessorName} = {returnType.PropertyAccessorInitializerSnippet(strategy.CompiletimeTypeSnippet(), member.Name)};
        """;

    public override string MemberDeclaration(int indentSize) =>
        $"""
        {Indent(indentSize)}{SerializeAttributes(member.GetCustomAttributesData(), indentSize)}public {returnType.ReturnTypeSnippet()} {member.Name} => {returnType.ToConversionSnippet($"{accessorName}(wrappedInstance)")};
        """;
}
