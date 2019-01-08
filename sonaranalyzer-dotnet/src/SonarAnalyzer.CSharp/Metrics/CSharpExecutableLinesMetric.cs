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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

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

        private class ExecutableLinesWalker : CSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;

            public ExecutableLinesWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public HashSet<int> ExecutableLineNumbers { get; }
                 = new HashSet<int>();

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
                        return !HasExcludedCodeAttribute((BaseTypeDeclarationSyntax)node, btdc => btdc.AttributeLists,
                            canBePartial: true);

                    case SyntaxKind.MethodDeclaration:
                    case SyntaxKind.ConstructorDeclaration:
                        return !HasExcludedCodeAttribute((BaseMethodDeclarationSyntax)node, bmds => bmds.AttributeLists,
                            canBePartial: true);

                    case SyntaxKind.PropertyDeclaration:
                    case SyntaxKind.EventDeclaration:
                        return !HasExcludedCodeAttribute((BasePropertyDeclarationSyntax)node, bpds => bpds.AttributeLists);

                    case SyntaxKind.AddAccessorDeclaration:
                    case SyntaxKind.RemoveAccessorDeclaration:
                    case SyntaxKind.SetAccessorDeclaration:
                    case SyntaxKind.GetAccessorDeclaration:
                        return !HasExcludedCodeAttribute((AccessorDeclarationSyntax)node, ads => ads.AttributeLists);

                    default:
                        return true;
                }
            }

            private bool HasExcludedCodeAttribute<T>(T node, Func<T, SyntaxList<AttributeListSyntax>> getAttributeLists,
                bool canBePartial = false)
                where T : SyntaxNode
            {
                var hasExcludeFromCodeCoverageAttribute = getAttributeLists(node)
                    .SelectMany(attributeList => attributeList.Attributes)
                    .Any(HasExcludedAttribute);

                if (!canBePartial)
                {
                    return hasExcludeFromCodeCoverageAttribute;
                }

                var nodeSymbol = this.semanticModel.GetDeclaredSymbol(node);
                switch (nodeSymbol?.Kind)
                {
                    case SymbolKind.Method:
                    case SymbolKind.NamedType:
                        return hasExcludeFromCodeCoverageAttribute ||
                            nodeSymbol.GetAttributes(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute).Any();

                    default:
                        return hasExcludeFromCodeCoverageAttribute;
                }
            }

            private static bool HasExcludedAttribute(AttributeSyntax attribute)
            {
                var attributeName = attribute?.Name?.ToString() ?? string.Empty;

                // Check the attribute name without the attribute suffix OR the full name of the attribute
                return attributeName.EndsWith(
                        KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName.Substring(0,
                            KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName.Length - 9), StringComparison.Ordinal) ||
                    attributeName.EndsWith(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName, StringComparison.Ordinal);
            }
        }
    }
}
