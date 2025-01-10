/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

public class VisualBasicBuilderPatternCondition : BuilderPatternCondition<SyntaxKind, InvocationExpressionSyntax>
{
    public VisualBasicBuilderPatternCondition(bool constructorIsSafe, params BuilderPatternDescriptor<SyntaxKind, InvocationExpressionSyntax>[] descriptors)
        : base(constructorIsSafe, descriptors, new VisualBasicAssignmentFinder()) { }

    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override SyntaxNode GetExpression(InvocationExpressionSyntax node) =>
        node.Expression;

    protected override string GetIdentifierName(InvocationExpressionSyntax node) =>
        node.Expression.GetName();

    protected override bool IsMemberAccess(SyntaxNode node, out SyntaxNode memberAccessExpression)
    {
        if (node is MemberAccessExpressionSyntax memberAccess)
        {
            memberAccessExpression = memberAccess.Expression;
            return true;
        }
        memberAccessExpression = null;
        return false;
    }

    protected override bool IsObjectCreation(SyntaxNode node) =>
        node is ObjectCreationExpressionSyntax;

    protected override bool IsIdentifier(SyntaxNode node, out string identifierName)
    {
        if (node is IdentifierNameSyntax identifier)
        {
            identifierName = identifier.Identifier.ValueText;
            return true;
        }
        identifierName = null;
        return false;
    }
}
