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

namespace SonarAnalyzer.Rules.VisualBasic
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
            && HaveSameParameters(model, firstMethod.GetParameters(), secondMethod.GetParameters())
            && VisualBasicEquivalenceChecker.AreEquivalent(firstMethod.Statements, secondMethod.Statements);

        protected override SyntaxToken GetMethodIdentifier(MethodBlockSyntax method) =>
            method.SubOrFunctionStatement.Identifier;
    }
}
