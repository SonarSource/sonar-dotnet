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
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Metrics.VisualBasic
{
    public static class VisualBasicExecutableLinesMetric
    {
        public static ImmutableArray<int> GetLineNumbers(SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            var walker = new ExecutableLinesWalker(semanticModel);
            walker.SafeVisit(syntaxTree.GetRoot());

            return walker.ExecutableLines.ToImmutableArray();
        }

        private class ExecutableLinesWalker : VisualBasicSyntaxWalker
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

            private static bool HasExcludedAttribute(AttributeSyntax attribute)
            {
                var attributeName = attribute?.Name?.ToString() ?? string.Empty;

                // Check the attribute name without the attribute suffix OR the full name of the attribute
                return attributeName.EndsWith(
                        KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName.Substring(0,
                            KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName.Length - 9), StringComparison.Ordinal) ||
                    attributeName.EndsWith(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute.ShortName, StringComparison.Ordinal);
            }

            private bool FindExecutableLines(SyntaxNode node)
            {
                switch (node.Kind())
                {
                    // The following C# constructs have no equivalent in VB.NET:
                    // - checked
                    // - unchecked
                    // - fixed
                    // - unsafe

                    case SyntaxKind.AttributeList:
                        return false;

                    case SyntaxKind.SyncLockStatement:
                    case SyntaxKind.UsingStatement:

                    case SyntaxKind.EmptyStatement:
                    case SyntaxKind.ExpressionStatement:
                    case SyntaxKind.SimpleAssignmentStatement:

                    case SyntaxKind.DoUntilStatement:
                    case SyntaxKind.DoWhileStatement:
                    case SyntaxKind.ForEachStatement:
                    case SyntaxKind.ForStatement:
                    case SyntaxKind.WhileStatement:

                    case SyntaxKind.IfStatement:
                    case SyntaxKind.LabelStatement:
                    case SyntaxKind.SelectStatement:
                    case SyntaxKind.ConditionalAccessExpression:
                    case SyntaxKind.BinaryConditionalExpression:
                    case SyntaxKind.TernaryConditionalExpression:

                    case SyntaxKind.GoToStatement:
                    case SyntaxKind.ThrowStatement:
                    case SyntaxKind.ReturnStatement:
                    case SyntaxKind.ExitDoStatement:
                    case SyntaxKind.ExitForStatement:
                    case SyntaxKind.ExitFunctionStatement:
                    case SyntaxKind.ExitOperatorStatement:
                    case SyntaxKind.ExitPropertyStatement:
                    case SyntaxKind.ExitSelectStatement:
                    case SyntaxKind.ExitSubStatement:
                    case SyntaxKind.ExitTryStatement:
                    case SyntaxKind.ExitWhileStatement:
                    case SyntaxKind.ContinueDoStatement:
                    case SyntaxKind.ContinueForStatement:

                    case SyntaxKind.YieldStatement:

                    case SyntaxKind.SimpleMemberAccessExpression:
                    case SyntaxKind.InvocationExpression:

                    case SyntaxKind.SingleLineSubLambdaExpression:
                    case SyntaxKind.SingleLineFunctionLambdaExpression:
                    case SyntaxKind.MultiLineSubLambdaExpression:
                    case SyntaxKind.MultiLineFunctionLambdaExpression:
                        this.executableLineNumbers.Add(node.GetLocation().GetLineNumberToReport());
                        return true;

                    case SyntaxKind.StructureStatement:
                    case SyntaxKind.ClassStatement:
                    case SyntaxKind.ModuleStatement:
                        return !HasExcludedCodeAttribute((TypeStatementSyntax)node, tss => tss.AttributeLists,
                            canBePartial: true);

                    case SyntaxKind.FunctionStatement:
                    case SyntaxKind.SubStatement:
                    case SyntaxKind.SubNewStatement:
                        return !HasExcludedCodeAttribute((MethodBaseSyntax)node, mbs => mbs.AttributeLists,
                            canBePartial: true);

                    case SyntaxKind.PropertyStatement:
                    case SyntaxKind.EventStatement:
                        return !HasExcludedCodeAttribute((MethodBaseSyntax)node, mbs => mbs.AttributeLists,
                            canBePartial: false);

                    case SyntaxKind.AddHandlerAccessorStatement:
                    case SyntaxKind.RemoveHandlerAccessorStatement:
                    case SyntaxKind.SetAccessorStatement:
                    case SyntaxKind.GetAccessorStatement:
                        return !HasExcludedCodeAttribute((AccessorStatementSyntax)node, ass => ass.AttributeLists,
                            canBePartial: false);

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
        }
    }
}
