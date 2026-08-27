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

public sealed class PropertyPassthroughSnippet : Snippet<PropertyInfo>
{
    public PropertyPassthroughSnippet(Strategy strategy, MemberDescriptor member, Strategy returnType) : base(strategy, member, returnType) { }

    public override string AccessorDeclaration() =>
        null;

    public override string MemberDeclaration(int indentSize)
    {
        var prefix = $"{Indent(indentSize)}{SerializeAttributes(member.GetCustomAttributesData(), indentSize)}public ";
        return member.GetMethod.IsStatic
            ? $"""{prefix}static {returnType.CompiletimeTypeSnippet} {member.Name} => {strategy.CompiletimeTypeSnippet}.{member.Name};"""
            : $"""{prefix}{returnType.CompiletimeTypeSnippet} {member.Name} => wrappedInstance.{member.Name};""";
    }
}
