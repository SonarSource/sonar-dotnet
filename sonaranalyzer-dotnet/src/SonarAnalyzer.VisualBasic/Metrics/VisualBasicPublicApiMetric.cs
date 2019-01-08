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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Metrics.VisualBasic
{
    public static class VisualBasicPublicApiMetric
    {
        public static ImmutableArray<SyntaxNode> GetMembers(SyntaxTree syntaxTree)
        {
            var walker = new PublicApiWalker();
            walker.SafeVisit(syntaxTree.GetRoot());
            return walker.PublicApi;
        }

        private class PublicApiWalker : VisualBasicSyntaxWalker
        {
            private readonly ImmutableArray<SyntaxNode>.Builder publicApi
                = ImmutableArray.CreateBuilder<SyntaxNode>();

            public ImmutableArray<SyntaxNode> PublicApi => this.publicApi.ToImmutable();

            public override void VisitClassBlock(ClassBlockSyntax node)
            {
                if (TryAddPublicApi(node.ClassStatement))
                {
                    base.VisitClassBlock(node);
                }
            }

            public override void VisitDelegateStatement(DelegateStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitEnumStatement(EnumStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitEventStatement(EventStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitInterfaceBlock(InterfaceBlockSyntax node)
            {
                if (TryAddPublicApi(node.InterfaceStatement))
                {
                    base.VisitInterfaceBlock(node);
                }
            }

            public override void VisitMethodStatement(MethodStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitModuleBlock(ModuleBlockSyntax node)
            {
                if (TryAddPublicApi(node.ModuleStatement))
                {
                    base.VisitModuleBlock(node);
                }
            }

            public override void VisitOperatorStatement(OperatorStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitPropertyStatement(PropertyStatementSyntax node) =>
                TryAddPublicApi(node);

            public override void VisitStructureBlock(StructureBlockSyntax node)
            {
                if (TryAddPublicApi(node.StructureStatement))
                {
                    base.VisitStructureBlock(node);
                }
            }

            public override void VisitSubNewStatement(SubNewStatementSyntax node) =>
                TryAddPublicApi(node);

            private bool IsPublic(SyntaxNode statement)
            {
                var isPublic = statement.ChildTokens().AnyOfKind(SyntaxKind.PublicKeyword);

                // statement is a declaration statement - method, class, etc. its Parent is
                // declaration block, its Parent is another block.
                var parentInterfaceBlock = statement.Parent?.Parent;

                // Interface members are public even though they don't have modifiers
                if (!isPublic &&
                    parentInterfaceBlock != null &&
                    parentInterfaceBlock.IsKind(SyntaxKind.InterfaceBlock))
                {
                    isPublic = IsPublic(((InterfaceBlockSyntax)parentInterfaceBlock).InterfaceStatement);
                }

                return isPublic;
            }

            private bool TryAddPublicApi(SyntaxNode node)
            {
                if (IsPublic(node))
                {
                    this.publicApi.Add(node);
                    return true;
                }
                return false;
            }
        }
    }
}
