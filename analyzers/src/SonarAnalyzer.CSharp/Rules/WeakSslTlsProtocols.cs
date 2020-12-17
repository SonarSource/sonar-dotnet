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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class WeakSslTlsProtocols : WeakSslTlsProtocolsBase<SyntaxKind, IdentifierNameSyntax>
    {
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } = CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxKind SyntaxKindOfInterest { get; } = SyntaxKind.IdentifierName;

        protected override DiagnosticDescriptor Rule { get; } = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override string GetIdentifierText(IdentifierNameSyntax identifierNameSyntax) =>
            identifierNameSyntax.Identifier.Text;

        protected override bool IsNodeOfInterest(SyntaxNode node)
        {
            if (!(node.Parent is MemberAccessExpressionSyntax))
            {
                return false;
            }

            var topNode = node.Parent.GetSelfOrTopParenthesizedExpression();

            if (topNode.Parent != null && topNode.Parent.IsKind(SyntaxKind.BitwiseNotExpression))
            {
                return false;
            }

            var current = topNode;
            var parent = current.Parent;
            while (parent != null && !parent.IsKind(SyntaxKind.IfStatement))
            {
                current = parent;
                parent = parent.Parent;
            }

            if (parent != null && parent.IsKind(SyntaxKind.IfStatement) && current != null)
            {
                return (parent as IfStatementSyntax).Condition != current;
            }

            return true;
        }
    }
}
