/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

        private static readonly string[] ExcludeAttrs =
            {
                "ExcludeFromCodeCoverage",
                "ExcludeFromCodeCoverageAttribute"
            };

        public ICollection<int> ExecutableLines => executableLineNumbers;

        public override void DefaultVisit(SyntaxNode node)
        {
            if (FindExecutableLines(node))
            {
                base.DefaultVisit(node);
            }
        }

        private bool FindExecutableLines(SyntaxNode node)
        {
            if (HasExcludedCodeAttribute(node))
            {
                return false;
            }

            var kind = node.Kind();
            switch (kind)
            {
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
                    executableLineNumbers.Add(node.GetLocation().GetLineNumberToReport());
                    return true;

                default:
                    return true;
            }
        }

        private static bool HasExcludedCodeAttribute(SyntaxNode node)
        {
            return node
                .DescendantNodesAndSelf()
                .Select(GetAttributeName)
                .Any(IsExcludedAttribute);
        }

        private static string GetAttributeName(SyntaxNode node)
        {
            var attribute = node as AttributeSyntax;
            return attribute?.Name.ToString() ?? string.Empty;
        }

        private static bool IsExcludedAttribute(string attributeName)
        {
            return ExcludeAttrs.Any(attr =>
                attributeName.EndsWith(attr, StringComparison.Ordinal));
        }
    }
}
