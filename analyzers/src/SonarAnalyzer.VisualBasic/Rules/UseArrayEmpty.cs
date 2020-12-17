/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class UseArrayEmpty : UseArrayEmptyBase<SyntaxKind, ObjectCreationExpressionSyntax, CollectionInitializerSyntax>
    {
        public UseArrayEmpty() : base(RspecStrings.ResourceManager) { }

        protected override SyntaxKind[] SyntaxKindsOfInterest => new[]
        {
            SyntaxKind.VariableDeclarator,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.CollectionInitializer,
        };

        protected override string ArrayEmptySuffix => "(Of T)";

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer
           => Helpers.VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        protected override bool ShouldReport(SyntaxNode node)
            => base.ShouldReport(node)
            || (node is VariableDeclaratorSyntax variableDeclaratorNode
            && IsEmpyVariableDeclarator(variableDeclaratorNode));

        private bool IsEmpyVariableDeclarator(VariableDeclaratorSyntax variableDeclaratorNode)
        {
            var arguments = variableDeclaratorNode
                .ChildNodes()
                .OfType<ModifiedIdentifierSyntax>()
                .FirstOrDefault()?
                .ArrayBounds?
                .ChildNodes()
                .OfType<SimpleArgumentSyntax>()
                .ToArray();

            return arguments?.Length == 1
                && int.TryParse(arguments[0].ToFullString(), out var arg)
                && arg == -1;
        }

        protected override bool IsEmptyCreation(ObjectCreationExpressionSyntax creationNode)
            => int.TryParse(SimpleArugment(creationNode)?.ToString(), out var count)
            && count == 0;

        private static SimpleArgumentSyntax SimpleArugment(ObjectCreationExpressionSyntax creationNode) => creationNode
            .ChildNodes()
            .OfType<ArgumentListSyntax>()
            .FirstOrDefault()?
            .Arguments.Cast<SimpleArgumentSyntax>()
            .FirstOrDefault();
    }
}

