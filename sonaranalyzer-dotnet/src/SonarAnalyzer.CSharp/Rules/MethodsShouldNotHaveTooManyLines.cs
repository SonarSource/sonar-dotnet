/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveTooManyLines
        : MethodsShouldNotHaveTooManyLinesBase<SyntaxKind, BaseMethodDeclarationSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
            new[]
            {
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration
            };

        protected override string MethodKeyword { get; } = "methods";

        protected override IEnumerable<SyntaxToken> GetMethodTokens(BaseMethodDeclarationSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.ExpressionBody()?.Expression?.DescendantTokens()
                ?? baseMethodDeclaration.Body?.Statements.SelectMany(s => s.DescendantTokens())
                ?? Enumerable.Empty<SyntaxToken>();

        protected override SyntaxToken? GetMethodIdentifierToken(BaseMethodDeclarationSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.GetIdentifierOrDefault();

        protected override string GetMethodKindAndName(SyntaxToken identifierToken)
        {
            var identifierName = identifierToken.ValueText;
            if (string.IsNullOrEmpty(identifierName))
            {
                return "method";
            }

            var declaration = identifierToken.Parent;
            if (declaration.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                return $"constructor '{identifierName}'";
            }

            if (declaration.IsKind(SyntaxKind.DestructorDeclaration))
            {
                return $"finalizer '~{identifierName}'";
            }

            if (declaration is MethodDeclarationSyntax)
            {
                return $"method '{identifierName}'";
            }

            return "method";
        }
    }
}
