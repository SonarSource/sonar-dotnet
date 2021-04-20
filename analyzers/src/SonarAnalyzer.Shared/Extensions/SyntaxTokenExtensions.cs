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

using Microsoft.CodeAnalysis;

#if CS
using Microsoft.CodeAnalysis.CSharp.Syntax;
#else
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif

namespace SonarAnalyzer.Extensions
{
    internal static class SyntaxTokenExtensions
    {
        // Based on Roslyn: https://github.com/dotnet/roslyn/blob/09903da31892a30f71eab67d2fd83232cfbf0cea/src/Workspaces/CSharp/Portable/LanguageServices/CSharpSyntaxFactsService.cs#L1187-L1254
        public static SyntaxNode GetBindableParent(this SyntaxToken token)
        {
            var node = token.Parent;

            while (node != null)
            {
                var parent = node.Parent;

                switch (parent)
                {
                    // If this node is on the left side of a member access expression, don't ascend
                    // further or we'll end up binding to something else.
                    case MemberAccessExpressionSyntax memberAccess when memberAccess.Expression == node:
                        return node;

                    // If this node is on the left side of a qualified name, don't ascend
                    // further or we'll end up binding to something else.
                    case QualifiedNameSyntax qualifiedName when qualifiedName.Left == node:
                        return node;

                    // If this node is the type of an object creation expression, return the
                    // object creation expression.
                    case ObjectCreationExpressionSyntax objectCreation when objectCreation.Type == node:
                        return parent;

                    // The inside of an interpolated string is treated as its own token so we
                    // need to force navigation to the parent expression syntax.
                    case InterpolatedStringExpressionSyntax _ when node is InterpolatedStringTextSyntax:
                        return parent;

                    case NameSyntax _:
                        node = parent;
                        break;

                    // If this node is not parented by a name, we're done.
                    default:
                        return node;
                }
            }

            return null;
        }
    }
}
