/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class SillyBitwiseOperationCodeFixProvider : SillyBitwiseOperationCodeFixProviderBase
    {
        protected override Func<SyntaxNode> CreateNewRoot(SyntaxNode root, TextSpan diagnosticSpan, bool isReportingOnLeft)
        {
            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is BinaryExpressionSyntax binary)
            {
                var newNode = isReportingOnLeft ? binary.Right : binary.Left;
                return () => root.ReplaceNode(binary, newNode.WithTrailingTrivia(binary.GetTrailingTrivia()).WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return null;
            }
        }
    }
}
