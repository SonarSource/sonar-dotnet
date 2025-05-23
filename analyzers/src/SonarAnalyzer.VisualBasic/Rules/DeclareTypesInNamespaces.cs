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

namespace SonarAnalyzer.Rules.VisualBasic
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
