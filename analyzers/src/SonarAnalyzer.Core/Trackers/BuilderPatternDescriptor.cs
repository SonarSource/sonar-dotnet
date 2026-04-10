/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

public class BuilderPatternDescriptor<TSyntaxKind, TInvocationSyntax>
    where TSyntaxKind : struct
    where TInvocationSyntax : SyntaxNode
{
    private readonly TrackerBase<TSyntaxKind, InvocationContext>.Condition[] invocationConditions;
    private readonly Func<TInvocationSyntax, bool> isValid;

    public BuilderPatternDescriptor(bool isValid, params TrackerBase<TSyntaxKind, InvocationContext>.Condition[] invocationConditions) : this(_ => isValid, invocationConditions) { }

    public BuilderPatternDescriptor(Func<TInvocationSyntax, bool> isValid, params TrackerBase<TSyntaxKind, InvocationContext>.Condition[] invocationConditions)
    {
        this.isValid = isValid;
        this.invocationConditions = invocationConditions;
    }

    public bool IsMatch(InvocationContext context) =>
        invocationConditions.All(x => x(context));

    public bool IsValid(TInvocationSyntax invocation) =>
        isValid(invocation);
}
