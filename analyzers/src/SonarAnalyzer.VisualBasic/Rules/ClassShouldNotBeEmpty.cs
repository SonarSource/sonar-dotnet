/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

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
