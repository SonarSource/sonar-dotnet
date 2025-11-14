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

namespace SonarAnalyzer.CSharp.Walkers;

internal class ParameterValidationInMethodWalker : SafeCSharpSyntaxWalker
{
    private static readonly ISet<SyntaxKind> SubMethodEquivalents =
        new HashSet<SyntaxKind>
        {
            SyntaxKindEx.LocalFunctionStatement,        // Local function
            SyntaxKind.SimpleLambdaExpression,          // Action
            SyntaxKind.ParenthesizedLambdaExpression    // Func
        };

    private readonly SemanticModel model;
    private readonly List<SecondaryLocation> argumentExceptionLocations = new();

    protected bool keepWalking = true;

    public IEnumerable<SecondaryLocation> ArgumentExceptionLocations => argumentExceptionLocations;

    protected virtual string SecondaryMessage => null;

    public ParameterValidationInMethodWalker(SemanticModel model) =>
        this.model = model;

    public override void Visit(SyntaxNode node)
    {
        if (keepWalking
            && !node.IsAnyKind(SubMethodEquivalents))  // Don't explore deeper if this node is equivalent to a method declaration
        {
            base.Visit(node);
        }
    }

    public override void VisitThrowStatement(ThrowStatementSyntax node)
    {
        // When throw is like `throw new XXX` where XXX derives from ArgumentException, save location
        if (node.Expression is not null
            && model.GetTypeInfo(node.Expression) is var typeInfo
            && typeInfo.Type.DerivesFrom(KnownType.System_ArgumentException))
        {
            argumentExceptionLocations.Add(node.Expression.ToSecondaryLocation(SecondaryMessage));
        }

        // there is no need to visit children
    }

    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (node.IsMemberAccessOnKnownType("ThrowIfNull", KnownType.System_ArgumentNullException, model))
        {
            // "ThrowIfNull" returns void so it cannot be an argument. We can stop.
            argumentExceptionLocations.Add(node.ToSecondaryLocation(SecondaryMessage));
        }
        else
        {
            // Need to check the children of this node because of the pattern (await SomeTask()).Invocation()
            base.VisitInvocationExpression(node);
        }
    }
}
