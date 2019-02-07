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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class MethodsShouldNotHaveTooManyLines
        : MethodsShouldNotHaveTooManyLinesBase<SyntaxKind, MethodBlockBaseSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer =>
            VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind[] SyntaxKinds { get; } =
            new[]
            {
                SyntaxKind.ConstructorBlock,
                SyntaxKind.SubBlock,
                SyntaxKind.FunctionBlock
            };

        protected override string MethodKeyword { get; } = "procedures";

        protected override IEnumerable<SyntaxToken> GetMethodTokens(MethodBlockBaseSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.Statements.SelectMany(s => s.DescendantTokens())
                ?? Enumerable.Empty<SyntaxToken>();

        protected override SyntaxToken? GetMethodIdentifierToken(MethodBlockBaseSyntax baseMethodDeclaration) =>
            baseMethodDeclaration.GetIdentifierOrDefault();

        protected override string GetMethodKindAndName(SyntaxToken identifierToken)
        {
            var declaration = identifierToken.Parent;
            if (declaration.IsKind(SyntaxKind.SubNewStatement))
            {
                return $"constructor";
            }

            var identifierName = identifierToken.ValueText;
            if (string.IsNullOrEmpty(identifierName))
            {
                return "procedure";
            }

            if (declaration.IsKind(SyntaxKind.FunctionStatement))
            {
                return $"function '{identifierName}'";
            }

            if (declaration.IsKind(SyntaxKind.SubStatement))
            {
                return identifierName == "Finalize"
                    ? "finalizer"
                    : $"method '{identifierName}'";
            }

            return "procedure";
        }
    }
}

