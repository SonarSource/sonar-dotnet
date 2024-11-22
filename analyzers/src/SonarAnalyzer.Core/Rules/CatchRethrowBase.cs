/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class CatchRethrowBase<TSyntaxKind, TCatchClause> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TCatchClause : SyntaxNode
{
    internal const string DiagnosticId = "S2737";

    protected abstract TCatchClause[] AllCatches(SyntaxNode node);
    protected abstract SyntaxNode DeclarationType(TCatchClause catchClause);
    protected abstract bool HasFilter(TCatchClause catchClause);
    protected abstract bool ContainsOnlyThrow(TCatchClause currentCatch);

    protected override string MessageFormat => "Add logic to this catch clause or eliminate it and rethrow the exception automatically.";

    protected CatchRethrowBase() : base(DiagnosticId) { }

    protected void RaiseOnInvalidCatch(SonarSyntaxNodeReportingContext context)
    {
        var catches = AllCatches(context.Node);
        var caughtExceptionTypes = new Lazy<INamedTypeSymbol[]>(() => ComputeExceptionTypes(catches, context.SemanticModel));
        var redundantCatches = new HashSet<TCatchClause>();
        // We handle differently redundant catch clauses (just throw inside) that are followed by a non-redundant catch clause, because if they are removed, the method behavior will change.
        var followingCatchesOnlyThrow = true;

        for (var i = catches.Length - 1; i >= 0; i--)
        {
            var currentCatch = catches[i];
            if (ContainsOnlyThrow(currentCatch))
            {
                if (!HasFilter(currentCatch)
                    // Make sure we report only catch clauses that will not change the method behavior if removed.
                    && (followingCatchesOnlyThrow || IsRedundantToFollowingCatches(i, catches, caughtExceptionTypes.Value, redundantCatches)))
                {
                    redundantCatches.Add(currentCatch);
                }
            }
            else
            {
                followingCatchesOnlyThrow = false;
            }
        }

        foreach (var redundantCatch in redundantCatches)
        {
            context.ReportIssue(Rule, redundantCatch);
        }
    }

    private static bool IsRedundantToFollowingCatches(int catchIndex, TCatchClause[] catches, INamedTypeSymbol[] caughtExceptionTypes, ISet<TCatchClause> redundantCatches)
    {
        var currentType = caughtExceptionTypes[catchIndex];
        for (var i = catchIndex + 1; i < caughtExceptionTypes.Length; i++)
        {
            var nextCatchType = caughtExceptionTypes[i];
            if (nextCatchType is null || currentType.DerivesOrImplements(nextCatchType))
            {
                return redundantCatches.Contains(catches[i]);
            }
        }
        return true;
    }

    private INamedTypeSymbol[] ComputeExceptionTypes(IEnumerable<TCatchClause> catches, SemanticModel model) =>
        catches.Select(x => DeclarationType(x) is { } declarationType ? model.GetTypeInfo(declarationType).Type as INamedTypeSymbol : null).ToArray();
}
