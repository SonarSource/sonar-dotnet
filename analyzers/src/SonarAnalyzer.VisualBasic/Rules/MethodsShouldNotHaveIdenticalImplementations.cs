/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class MethodsShouldNotHaveIdenticalImplementations : MethodsShouldNotHaveIdenticalImplementationsBase<SyntaxKind, MethodBlockSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds => new[] { SyntaxKind.ClassBlock, SyntaxKind.StructureBlock };

        protected override IEnumerable<MethodBlockSyntax> GetMethodDeclarations(SyntaxNode node) =>
            ((TypeBlockSyntax)node).Members.OfType<MethodBlockSyntax>();

        protected override bool AreDuplicates(SemanticModel model, MethodBlockSyntax firstMethod, MethodBlockSyntax secondMethod) =>
            firstMethod.Statements.Count > 1
            && firstMethod.GetIdentifierText() != secondMethod.GetIdentifierText()
            && HaveSameParameters(firstMethod.GetParameters(), secondMethod.GetParameters())
            && HaveSameTypeParameters(model, firstMethod.SubOrFunctionStatement?.TypeParameterList?.Parameters, secondMethod.SubOrFunctionStatement?.TypeParameterList?.Parameters)
            && AreTheSameType(model, firstMethod.SubOrFunctionStatement.AsClause?.Type, secondMethod.SubOrFunctionStatement.AsClause?.Type)
            && VisualBasicEquivalenceChecker.AreEquivalent(firstMethod.Statements, secondMethod.Statements);

        protected override SyntaxToken GetMethodIdentifier(MethodBlockSyntax method) =>
            method.SubOrFunctionStatement.Identifier;
    }
}
