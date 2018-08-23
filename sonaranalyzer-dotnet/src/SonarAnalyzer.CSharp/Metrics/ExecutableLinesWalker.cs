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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Metrics.CSharp
{
    public class ExecutableLinesWalker : CSharpSyntaxWalker
    {
        private readonly HashSet<int> executableLineNumbers = new HashSet<int>();
        private readonly SemanticModel semanticModel;

        public ExecutableLinesWalker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public ICollection<int> ExecutableLines => this.executableLineNumbers;

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
                    this.executableLineNumbers.Add(node.GetLocation().GetLineNumberToReport());
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

            switch (this.semanticModel.GetDeclaredSymbol(node))
            {
                case IMethodSymbol methodSymbol:
                    return hasExcludeFromCodeCoverageAttribute ||
                        methodSymbol.GetAttributes().Any(HasExcludedAttribute);

                case INamedTypeSymbol namedTypeSymbol:
                    return hasExcludeFromCodeCoverageAttribute ||
                        namedTypeSymbol.GetAttributes().Any(HasExcludedAttribute);

                default:
                    return hasExcludeFromCodeCoverageAttribute;
            }
        }

        private static bool HasExcludedAttribute(AttributeSyntax attribute)
        {
            var attributeName = attribute?.Name?.ToString() ?? string.Empty;
            return IsExcludedAttribute(attributeName);
        }

        private static bool HasExcludedAttribute(AttributeData attribute)
        {
            var attributeName = attribute?.AttributeClass?.Name ?? string.Empty;
            return IsExcludedAttribute(attributeName);
        }

        private static bool IsExcludedAttribute(string attributeName) =>
            attributeName.EndsWith("ExcludeFromCodeCoverage", StringComparison.Ordinal) ||
            attributeName.EndsWith("ExcludeFromCodeCoverageAttribute", StringComparison.Ordinal);
    }
}
