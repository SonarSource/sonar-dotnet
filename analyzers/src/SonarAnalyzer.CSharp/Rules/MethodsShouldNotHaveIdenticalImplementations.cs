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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodsShouldNotHaveIdenticalImplementations : MethodsShouldNotHaveIdenticalImplementationsBase<SyntaxKind, IMethodDeclaration>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds => new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.CompilationUnit
        };

        protected override IEnumerable<IMethodDeclaration> GetMethodDeclarations(SyntaxNode node) =>
            node.IsKind(SyntaxKind.CompilationUnit)
            ? ((CompilationUnitSyntax)node).GetMethodDeclarations()
            : ((TypeDeclarationSyntax)node).GetMethodDeclarations();

        protected override bool AreDuplicates(SemanticModel model, IMethodDeclaration firstMethod, IMethodDeclaration secondMethod) =>
            firstMethod is { Body: { Statements: { Count: > 1 } } }
            && firstMethod.Identifier.ValueText != secondMethod.Identifier.ValueText
            && HaveSameParameters(firstMethod.ParameterList?.Parameters, secondMethod.ParameterList?.Parameters)
            && HaveSameTypeParameters(model, firstMethod.TypeParameterList?.Parameters, secondMethod.TypeParameterList?.Parameters)
            && AreTheSameType(model, firstMethod.ReturnType, secondMethod.ReturnType)
            && firstMethod.Body.IsEquivalentTo(secondMethod.Body, false);

        protected override SyntaxToken GetMethodIdentifier(IMethodDeclaration method) =>
            method.Identifier;

        protected override bool IsExcludedFromBeingExamined(SonarSyntaxNodeReportingContext context) =>
            base.IsExcludedFromBeingExamined(context)
            && !context.IsTopLevelMain();
    }
}
