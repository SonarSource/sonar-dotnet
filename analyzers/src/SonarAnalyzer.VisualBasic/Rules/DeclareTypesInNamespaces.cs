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

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase<SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxKind[] SyntaxKinds { get; } =
    [
        SyntaxKind.ClassStatement,
        SyntaxKind.StructureStatement,
        SyntaxKind.EnumStatement,
        SyntaxKind.InterfaceStatement
    ];

    protected override SyntaxToken ResolveTypeIdentifier(SyntaxNode declaration) =>
        declaration.Kind() switch
        {
            SyntaxKind.EnumStatement => ((EnumStatementSyntax)declaration).Identifier,
            SyntaxKind.ClassStatement or SyntaxKind.InterfaceStatement or SyntaxKind.StructureStatement => ((TypeStatementSyntax)declaration).Identifier,
            _ => default,
        };

    protected override bool IsException(SyntaxNode node, SemanticModel model) => false;

    protected override bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel model) =>
        declaration.Parent.Parent.Kind() is SyntaxKind.ClassBlock or SyntaxKind.InterfaceBlock or SyntaxKind.StructureBlock or SyntaxKind.NamespaceBlock
        // If declaration is an outer type that is not within a namespace block, make sure there is no Root Namespace set in the project
        || model.GetDeclaredSymbol(declaration) is not INamedTypeSymbol { ContainingNamespace.IsGlobalNamespace: true };
}
