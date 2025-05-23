﻿/*
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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ClassShouldNotBeEmpty : ClassShouldNotBeEmptyBase<SyntaxKind, BaseTypeDeclarationSyntax>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override bool IsEmptyAndNotPartial(SyntaxNode node) =>
        node is TypeDeclarationSyntax { Members.Count: 0 } typeDeclaration
        && !typeDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword))
        && LacksParameterizedPrimaryConstructor(node);

    protected override BaseTypeDeclarationSyntax GetIfHasDeclaredBaseClassOrInterface(SyntaxNode node) =>
        node is TypeDeclarationSyntax { BaseList: not null } declaration
            ? declaration
            : null;

    // Unlike in VB.NET there's no way to know for certain - by only looking at the syntax tree - from the declared types whether they are classes or interfaces.
    // Unless the class has more than one base type, because then one of them has to be an interface, as there can't be more than one base class.
    protected override bool HasInterfaceOrGenericBaseClass(BaseTypeDeclarationSyntax declaration) =>
        declaration.BaseList.Types.Count > 1                                 // implements at least one interface
        || declaration.BaseList.Types.Any(x => x.Type is GenericNameSyntax); // or a generic class/interface

    protected override bool HasAnyAttribute(SyntaxNode node) =>
        node is TypeDeclarationSyntax { AttributeLists.Count: > 0 };

    protected override string DeclarationTypeKeyword(SyntaxNode node) =>
        ((TypeDeclarationSyntax)node).Keyword.ValueText;

    protected override bool HasConditionalCompilationDirectives(SyntaxNode node) =>
        node.DescendantNodes(descendIntoTrivia: true).Any(x => x.IsAnyKind(
            SyntaxKind.IfDirectiveTrivia,
            SyntaxKind.ElifDirectiveTrivia,
            SyntaxKind.ElseDirectiveTrivia,
            SyntaxKind.EndIfDirectiveTrivia));

    private static bool LacksParameterizedPrimaryConstructor(SyntaxNode node) =>
        IsParameterlessClass(node)
        || IsParameterlessRecord(node);

    private static bool IsParameterlessClass(SyntaxNode node) =>
        node is ClassDeclarationSyntax declaration
        && LacksParameters(declaration.ParameterList(), declaration.BaseList);

    private static bool IsParameterlessRecord(SyntaxNode node) =>
        RecordDeclarationSyntax(node) is { } declaration
        && LacksParameters(declaration.ParameterList, declaration.BaseList);

    private static bool LacksParameters(ParameterListSyntax parameterList, BaseListSyntax baseList) =>
        parameterList?.Parameters is not { Count: > 0 }
        && BaseTypeSyntax(baseList) is not { ArgumentList.Arguments.Count: > 0 };

    private static RecordDeclarationSyntaxWrapper? RecordDeclarationSyntax(SyntaxNode node) =>
        RecordDeclarationSyntaxWrapper.IsInstance(node)
            ? (RecordDeclarationSyntaxWrapper)node
            : null;

    private static PrimaryConstructorBaseTypeSyntaxWrapper? BaseTypeSyntax(BaseListSyntax list) =>
       list?.Types.FirstOrDefault() is { } type
       && PrimaryConstructorBaseTypeSyntaxWrapper.IsInstance(type)
           ? (PrimaryConstructorBaseTypeSyntaxWrapper)type
           : null;
}
