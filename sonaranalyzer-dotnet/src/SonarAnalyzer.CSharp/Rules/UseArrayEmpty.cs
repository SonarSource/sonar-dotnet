/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UseArrayEmpty : UseArrayEmptyBase<SyntaxKind, ArrayCreationExpressionSyntax, InitializerExpressionSyntax>
    {
        private static readonly DiagnosticDescriptor rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule => rule;
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.GeneratedCodeRecognizer.Instance;

        protected override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => new SyntaxKind[] 
        {
            SyntaxKind.ArrayCreationExpression,
            SyntaxKind.ArrayInitializerExpression,
        }.ToImmutableArray();

        protected override bool IsEmptyCreation(ArrayCreationExpressionSyntax node)
        {
            var rankSpecifier = node?.ChildNodes().OfType<ArrayTypeSyntax>().FirstOrDefault()?.ChildNodes().OfType<ArrayRankSpecifierSyntax>().FirstOrDefault();
            return rankSpecifier != null && rankSpecifier.Rank == 1 &&
                int.TryParse(rankSpecifier.Sizes[0].ToString(), out var size) &&
                size == 0;
        }
    }
}
