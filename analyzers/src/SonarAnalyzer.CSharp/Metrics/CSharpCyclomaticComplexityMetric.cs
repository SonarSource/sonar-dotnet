/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Metrics;

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

    public static CyclomaticComplexity GetComplexity(SyntaxNode syntaxNode) =>
        GetComplexity(syntaxNode, false);

    public static CyclomaticComplexity GetComplexity(SyntaxNode syntaxNode, bool onlyGlobalStatements)
    {
        var walker = new CyclomaticWalker(onlyGlobalStatements);
        if (syntaxNode.IsKind(SyntaxKindEx.LocalFunctionStatement))
        {
            walker.VisitLocalFunction((LocalFunctionStatementSyntaxWrapper)syntaxNode);
        }
        else
        {
            walker.SafeVisit(syntaxNode);
        }

        return new CyclomaticComplexity(walker.IncrementLocations.ToImmutableArray());
    }

    private sealed class CyclomaticWalker : SafeCSharpSyntaxWalker
    {
        private readonly bool onlyGlobalStatements;

        public List<SecondaryLocation> IncrementLocations { get; } = new();

        public CyclomaticWalker(bool onlyGlobalStatements) =>
            this.onlyGlobalStatements = onlyGlobalStatements;

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var globalStatement in node.Members.Where(x => x.IsKind(SyntaxKind.GlobalStatement)))
            {
                if (!IsStaticLocalFunction(globalStatement))
                {
                    base.Visit(globalStatement);
                }
            }

            if (!onlyGlobalStatements)
            {
                base.VisitCompilationUnit(node);
            }
        }

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

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKindEx.CoalesceAssignmentExpression))
            {
                AddLocation(node.OperatorToken);
            }

            base.VisitAssignmentExpression(node);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            AddLocation(node.Keyword);
            base.VisitCaseSwitchLabel(node);
        }

        public void VisitLocalFunction(LocalFunctionStatementSyntaxWrapper node)
        {
            AddLocation(node.Identifier);
            base.Visit(node.SyntaxNode);
        }

        public override void Visit(SyntaxNode node)
        {
            if (SwitchExpressionArmSyntaxWrapper.IsInstance(node))
            {
                var arm = (SwitchExpressionArmSyntaxWrapper)node;
                AddLocation(arm.EqualsGreaterThanToken);
            }
            else if (node?.Kind() is SyntaxKindEx.AndPattern or SyntaxKindEx.OrPattern)
            {
                var binaryPatternNode = (BinaryPatternSyntaxWrapper)node;
                AddLocation(binaryPatternNode.OperatorToken);
            }

            if (!IsStaticLocalFunction(node))
            {
                base.Visit(node);
            }
        }

        private void AddLocation(SyntaxToken node) => IncrementLocations.Add(new SecondaryLocation(node.GetLocation(), "+1"));

        private static bool HasBody(SyntaxNode node) => node.ChildNodes().AnyOfKind(SyntaxKind.Block);

        private static bool IsStaticLocalFunction(SyntaxNode node) =>
            node.IsKind(SyntaxKindEx.LocalFunctionStatement)
            && ((LocalFunctionStatementSyntaxWrapper)node).Modifiers.Any(SyntaxKind.StaticKeyword);
    }
}
