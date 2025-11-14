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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class DeclareTypesInNamespaces : DeclareTypesInNamespacesBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
        {
            SyntaxKind.ClassStatement,
            SyntaxKind.StructureStatement,
            SyntaxKind.EnumStatement,
            SyntaxKind.InterfaceStatement
        };

        protected override SyntaxToken GetTypeIdentifier(SyntaxNode declaration)
        {
            switch (declaration.Kind())
            {
                case SyntaxKind.EnumStatement:
                    return ((EnumStatementSyntax)declaration).Identifier;
                case SyntaxKind.ClassStatement:
                case SyntaxKind.InterfaceStatement:
                case SyntaxKind.StructureStatement:
                    return ((TypeStatementSyntax)declaration).Identifier;
                default:
                    return default;
            }
        }

        protected override bool IsException(SyntaxNode node) => false;

        protected override bool IsInnerTypeOrWithinNamespace(SyntaxNode declaration, SemanticModel semanticModel)
        {
            switch (declaration.Parent.Parent.Kind())
            {
                case SyntaxKind.ClassBlock:
                case SyntaxKind.InterfaceBlock:
                case SyntaxKind.StructureBlock:
                case SyntaxKind.NamespaceBlock:
                    return true;
                default:
                    // If declaration is an outer type that is not within a namespace block,
                    // make sure there is no Root Namespace set in the project
                    var typeSymbol = (INamedTypeSymbol)semanticModel.GetDeclaredSymbol(declaration);
                    return typeSymbol == null ||
                        !typeSymbol.ContainingNamespace.IsGlobalNamespace;
            }
        }
    }
}
