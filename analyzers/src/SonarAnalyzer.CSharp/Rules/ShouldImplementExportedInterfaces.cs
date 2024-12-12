/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ShouldImplementExportedInterfaces : ShouldImplementExportedInterfacesBase<AttributeArgumentSyntax, AttributeSyntax, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SeparatedSyntaxList<AttributeArgumentSyntax>? GetAttributeArguments(AttributeSyntax attributeSyntax) =>
            attributeSyntax.ArgumentList?.Arguments;

        protected override SyntaxNode GetAttributeName(AttributeSyntax attributeSyntax) =>
            attributeSyntax.Name;

        protected override bool IsClassOrRecordSyntax(SyntaxNode syntaxNode) =>
            syntaxNode?.Kind() is SyntaxKind.ClassDeclaration or SyntaxKindEx.RecordDeclaration;

        protected override SyntaxNode GetTypeOfOrGetTypeExpression(SyntaxNode expressionSyntax) =>
            (expressionSyntax as TypeOfExpressionSyntax)?.Type;
    }
}
