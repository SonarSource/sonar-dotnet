/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers.Trackers
{
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
            return new Condition(context =>
            {
                foreach (var baseTypeNode in context.AllBaseTypeNodes)
                {
                    if (context.SemanticModel.GetTypeInfo(baseTypeNode).Type.DerivesOrImplementsAny(immutableTypes))
                    {
                        context.PrimaryLocation = baseTypeNode.GetLocation();
                        return true; // assume there won't be more than one matching node
                    }
                }

                return false;
            });
        }

        protected override BaseTypeContext CreateContext(SyntaxNodeAnalysisContext context) =>
            GetBaseTypeNodes(context.Node) is { } baseTypeList
            && baseTypeList.Any()
            ? new BaseTypeContext(context, baseTypeList)
            : null;
    }
}
