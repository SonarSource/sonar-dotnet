/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Rules.VisualBasic;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class ClassShouldNotBeEmpty : ClassShouldNotBeEmptyBase<SyntaxKind, TypeBlockSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override bool IsEmptyAndNotPartial(SyntaxNode node) =>
        node is ClassBlockSyntax { Members.Count: 0 } classSyntax
        && !classSyntax.ClassStatement.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword));

    protected override TypeBlockSyntax GetIfHasDeclaredBaseClassOrInterface(SyntaxNode node) =>
        node is ClassBlockSyntax { Inherits.Count: > 0 } or ClassBlockSyntax { Implements.Count: > 0 }
            ? (ClassBlockSyntax)node
            : null;

    protected override bool HasInterfaceOrGenericBaseClass(TypeBlockSyntax declaration) =>
        declaration.Implements.Any()
        || declaration.Inherits.Any(x => x.Types.Any(t => t is GenericNameSyntax));

    protected override bool HasAnyAttribute(SyntaxNode node) =>
        node is ClassBlockSyntax { ClassStatement.AttributeLists.Count: > 0 };

    protected override bool HasConditionalCompilationDirectives(SyntaxNode node) =>
        node.DescendantNodes(descendIntoTrivia: true).Any(x => x.IsAnyKind(
            SyntaxKind.IfDirectiveTrivia,
            SyntaxKind.ElseIfDirectiveTrivia,
            SyntaxKind.ElseDirectiveTrivia,
            SyntaxKind.EndIfDirectiveTrivia));

    protected override string DeclarationTypeKeyword(SyntaxNode node) =>
        ((TypeBlockSyntax)node).BlockStatement.DeclarationKeyword.ValueText.ToLower();
}
