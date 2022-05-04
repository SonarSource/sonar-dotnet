/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Metrics.CSharp
{
    public static class CSharpExecutableLinesMetric
    {
        public static ImmutableArray<int> GetLineNumbers(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var walker = new ExecutableLinesWalker(semanticModel);
            walker.SafeVisit(syntaxTree.GetRoot());
            return walker.ExecutableLineNumbers.ToImmutableArray();
        }

        private sealed class ExecutableLinesWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel model;

            public HashSet<int> ExecutableLineNumbers { get; } = new();

            public ExecutableLinesWalker(SemanticModel model) =>
                this.model = model;

            public override void DefaultVisit(SyntaxNode node)
            {
                if (FindExecutableLines(node))
                {
                    base.DefaultVisit(node);
                }
            }

            private bool FindExecutableLines(SyntaxNode node)
            {
                switch (node.Kind())
                {
                    case SyntaxKind.AttributeList:
                        return false;

                    case SyntaxKind.CheckedStatement:
                    case SyntaxKind.UncheckedStatement:

                    case SyntaxKind.LockStatement:
                    case SyntaxKind.FixedStatement:
                    case SyntaxKind.UnsafeStatement:
                    case SyntaxKind.UsingStatement:

                    case SyntaxKind.EmptyStatement:
                    case SyntaxKind.ExpressionStatement:

                    case SyntaxKind.DoStatement:
                    case SyntaxKind.ForEachStatement:
                    case SyntaxKind.ForStatement:
                    case SyntaxKind.WhileStatement:

                    case SyntaxKind.IfStatement:
                    case SyntaxKind.LabeledStatement:
                    case SyntaxKind.SwitchStatement:
                    case SyntaxKind.ConditionalAccessExpression:
                    case SyntaxKind.ConditionalExpression:

                    case SyntaxKind.GotoStatement:
                    case SyntaxKind.ThrowStatement:
                    case SyntaxKind.ReturnStatement:
                    case SyntaxKind.BreakStatement:
                    case SyntaxKind.ContinueStatement:

                    case SyntaxKind.YieldBreakStatement:
                    case SyntaxKind.YieldReturnStatement:

                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.InvocationExpression:

                    case SyntaxKind.SimpleLambdaExpression:
                    case SyntaxKind.ParenthesizedLambdaExpression:

                    case SyntaxKind.ArrayInitializerExpression:
                        ExecutableLineNumbers.Add(node.GetLocation().GetLineNumberToReport());
                        return true;

                    case SyntaxKind.StructDeclaration:
                    case SyntaxKind.ClassDeclaration:
                    case SyntaxKindEx.RecordClassDeclaration:
                    case SyntaxKindEx.RecordStructDeclaration:
                        return !HasExcludedCodeAttribute((BaseTypeDeclarationSyntax)node, x => x.AttributeLists, true);

                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.ConstructorDeclaration:
                        return !HasExcludedCodeAttribute((BaseMethodDeclarationSyntax)node, x => x.AttributeLists, true);

                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.EventDeclaration:
                        return !HasExcludedCodeAttribute((BasePropertyDeclarationSyntax)node, x => x.AttributeLists, false);

                    case SyntaxKind.AddAccessorDeclaration:
                    case SyntaxKind.RemoveAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                    case SyntaxKind.GetAccessorDeclaration:
                    case SyntaxKindEx.InitAccessorDeclaration:
                        return !HasExcludedCodeAttribute((AccessorDeclarationSyntax)node, ads => ads.AttributeLists, false);

                    default:
                        return true;
                }
            }

            private bool HasExcludedCodeAttribute<T>(T node, Func<T, SyntaxList<AttributeListSyntax>> getAttributeLists, bool canBePartial) where T : SyntaxNode
            {
                var hasExcludeFromCodeCoverageAttribute = getAttributeLists(node).SelectMany(x => x.Attributes).Any(IsExcludedAttribute);
                return hasExcludeFromCodeCoverageAttribute || !canBePartial
                    ? hasExcludeFromCodeCoverageAttribute
                    : model.GetDeclaredSymbol(node) is { Kind: SymbolKind.Method or SymbolKind.NamedType} symbol
                      && symbol.HasAttribute(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute);
            }

            private bool IsExcludedAttribute(AttributeSyntax attribute) =>
                attribute.IsKnownType(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute, model);
        }
    }
}
