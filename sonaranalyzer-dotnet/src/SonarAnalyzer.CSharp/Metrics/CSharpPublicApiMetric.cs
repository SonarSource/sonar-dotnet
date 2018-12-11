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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Metrics
{
    public static class CSharpPublicApiMetric
    {
        public static ImmutableArray<SyntaxNode> GetMembers(SyntaxTree syntaxTree)
        {
            var root = syntaxTree.GetRoot();
            var publicNodes = ImmutableArray.CreateBuilder<SyntaxNode>();
            var toVisit = new Stack<SyntaxNode>();

            var members = root.ChildNodes()
                .Where(childNode => childNode is MemberDeclarationSyntax);
            foreach (var member in members)
            {
                toVisit.Push(member);
            }

            while (toVisit.Any())
            {
                var member = toVisit.Pop();

                var isPublic = member.ChildTokens().AnyOfKind(SyntaxKind.PublicKeyword);
                if (isPublic)
                {
                    publicNodes.Add(member);
                }

                if (!isPublic &&
                    !member.IsKind(SyntaxKind.NamespaceDeclaration))
                {
                    continue;
                }

                members = member.ChildNodes()
                    .Where(childNode => childNode is MemberDeclarationSyntax);
                foreach (var child in members)
                {
                    toVisit.Push(child);
                }
            }

            return publicNodes.ToImmutable();
        }
    }
}
