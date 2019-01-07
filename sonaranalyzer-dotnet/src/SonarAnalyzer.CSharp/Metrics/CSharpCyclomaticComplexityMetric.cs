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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Metrics.CSharp
{
    public static class CSharpCyclomaticComplexityMetric
    {
        public class CyclomaticComplexity
        {
            public CyclomaticComplexity(ImmutableArray<SecondaryLocation> locations)
            {
                Locations = locations;
            }

            public ImmutableArray<SecondaryLocation> Locations { get; }
            public int Complexity => Locations.Length;
        }

        public static CyclomaticComplexity GetComplexity(SyntaxNode syntaxNode)
        {
            var walker = new CyclomaticWalker();
            walker.SafeVisit(syntaxNode);

            return new CyclomaticComplexity(walker.IncrementLocations.ToImmutableArray());
        }

        private class CyclomaticWalker : CSharpSyntaxWalker
        {
            public List<SecondaryLocation> IncrementLocations { get; }
                = new List<SecondaryLocation>();

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.ExpressionBody != null || HasBody(node))
                {
                    AddLocation(node.Identifier);
                }
                base.VisitMethodDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (node.ExpressionBody != null || HasBody(node))
                {
                    AddLocation(node.Identifier);
                }
                base.VisitPropertyDeclaration(node);
            }

            public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
            {
                AddLocation(node.OperatorToken);
                base.VisitOperatorDeclaration(node);
            }

            public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                AddLocation(node.Identifier);
                base.VisitConstructorDeclaration(node);
            }

            public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
            {
                AddLocation(node.Identifier);
                base.VisitDestructorDeclaration(node);
            }

            public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
            {
                AddLocation(node.Keyword);
                base.VisitAccessorDeclaration(node);
            }

            public override void VisitIfStatement(IfStatementSyntax node)
            {
                AddLocation(node.IfKeyword);
                base.VisitIfStatement(node);
            }

            public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
            {
                AddLocation(node.QuestionToken);
                base.VisitConditionalExpression(node);
            }

            public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
            {
                AddLocation(node.OperatorToken);
                base.VisitConditionalAccessExpression(node);
            }

            public override void VisitWhileStatement(WhileStatementSyntax node)
            {
                AddLocation(node.WhileKeyword);
                base.VisitWhileStatement(node);
            }

            public override void VisitDoStatement(DoStatementSyntax node)
            {
                AddLocation(node.DoKeyword);
                base.VisitDoStatement(node);
            }

            public override void VisitForStatement(ForStatementSyntax node)
            {
                AddLocation(node.ForKeyword);
                base.VisitForStatement(node);
            }

            public override void VisitForEachStatement(ForEachStatementSyntax node)
            {
                AddLocation(node.ForEachKeyword);
                base.VisitForEachStatement(node);
            }

            public override void VisitBinaryExpression(BinaryExpressionSyntax node)
            {
                if (node.IsKind(SyntaxKind.CoalesceExpression) ||
                    node.IsKind(SyntaxKind.LogicalAndExpression) ||
                    node.IsKind(SyntaxKind.LogicalOrExpression))
                {
                    AddLocation(node.OperatorToken);
                }

                base.VisitBinaryExpression(node);
            }

            public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
            {
                AddLocation(node.Keyword);
                base.VisitCaseSwitchLabel(node);
            }

            private void AddLocation(SyntaxToken node)
            {
                IncrementLocations.Add(new SecondaryLocation(node.GetLocation(), "+1"));
            }

            private static bool HasBody(SyntaxNode node)
            {
                return node.ChildNodes().AnyOfKind(SyntaxKind.Block);
            }
        }
    }
}
