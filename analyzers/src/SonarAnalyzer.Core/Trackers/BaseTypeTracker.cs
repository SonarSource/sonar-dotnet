/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

/// <summary>
/// Tracker class for rules that check the inheritance tree for e.g. disallowed base classes.
/// </summary>
/// <typeparam name="TSyntaxKind">The syntax type.</typeparam>
public abstract class BaseTypeTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, BaseTypeContext>
    where TSyntaxKind : struct
{
    /// <summary>
    /// Extract the list of type syntax nodes for the base types/interface types.
    /// </summary>
    protected abstract IEnumerable<SyntaxNode> GetBaseTypeNodes(SyntaxNode contextNode);

    internal Condition MatchSubclassesOf(params KnownType[] types)
    {
        var immutableTypes = types.ToImmutableArray();
        return context =>
        {
            foreach (var baseTypeNode in context.AllBaseTypeNodes)
            {
                if (context.Model.GetTypeInfo(baseTypeNode).Type.DerivesOrImplementsAny(immutableTypes))
                {
                    context.PrimaryLocation = baseTypeNode.GetLocation();
                    return true; // assume there won't be more than one matching node
                }
            }

            return false;
        };
    }

    protected override BaseTypeContext CreateContext(SonarSyntaxNodeReportingContext context) =>
        GetBaseTypeNodes(context.Node) is { } baseTypeList
        && baseTypeList.Any()
        ? new BaseTypeContext(context, baseTypeList)
        : null;
}
