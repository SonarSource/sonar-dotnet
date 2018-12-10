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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                    return !IsExcludedFromCoverage(node, canBePartial: true);

                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.EventDeclaration:
                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                    return !IsExcludedFromCoverage(node, canBePartial: false);

                default:
                    return true;
            }
        }

        private bool IsExcludedFromCoverage(SyntaxNode node, bool canBePartial = false)
        {
            var hasExcludeFromCodeCoverageAttribute = node.GetAttributeLists()
                .GetAttributes(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute)
                .Any();

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
    }
}
