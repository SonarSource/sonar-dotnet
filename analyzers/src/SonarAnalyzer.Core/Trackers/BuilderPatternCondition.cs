﻿/*
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

namespace SonarAnalyzer.Core.Trackers;

public abstract class BuilderPatternCondition<TSyntaxKind, TInvocationSyntax>
    where TSyntaxKind : struct
    where TInvocationSyntax : SyntaxNode
{
    private readonly bool constructorIsSafe;
    private readonly BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>[] descriptors;
    private readonly AssignmentFinder assignmentFinder;

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
    protected abstract SyntaxNode GetExpression(TInvocationSyntax node);
    protected abstract string GetIdentifierName(TInvocationSyntax node);
    protected abstract bool IsMemberAccess(SyntaxNode node, out SyntaxNode memberAccessExpression);
    protected abstract bool IsObjectCreation(SyntaxNode node);
    protected abstract bool IsIdentifier(SyntaxNode node, out string identifierName);

    protected BuilderPatternCondition(bool constructorIsSafe, BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>[] descriptors, AssignmentFinder assignmentFinder)
    {
        this.constructorIsSafe = constructorIsSafe;
        this.descriptors = descriptors;
        this.assignmentFinder = assignmentFinder;
    }

    public bool IsInvalidBuilderInitialization(InvocationContext context)
    {
        var current = context.Node;
        while (current is not null)
        {
            current = Language.Syntax.RemoveParentheses(current);
            if (current is TInvocationSyntax invocation)
            {
                var invocationContext = new InvocationContext(invocation, GetIdentifierName(invocation), context.Model);
                if (descriptors.FirstOrDefault(x => x.IsMatch(invocationContext)) is { } descriptor)
                {
                    return !descriptor.IsValid(invocation);
                }
                current = GetExpression(invocation);
            }
            else if (IsMemberAccess(current, out var memberAccessExpression))
            {
                current = memberAccessExpression;
            }
            else if (IsObjectCreation(current))
            {
                // We're sure that full invocation chain started here => we've seen all configuration invocations.
                return !constructorIsSafe;
            }
            else if (IsIdentifier(current, out var identifierName))
            {
                if (!(context.Model.GetSymbolInfo(current).Symbol is ILocalSymbol))
                {
                    return false;
                }
                // When tracking reaches the local variable in invocation chain 'variable.MethodA().MethodB()'
                // we'll try to find preceding assignment to that variable to continue inspection of initialization chain.
                current = assignmentFinder.FindLinearPrecedingAssignmentExpression(identifierName, current);
            }
            else
            {
                return false;
            }
        }
        return false;
    }
}
