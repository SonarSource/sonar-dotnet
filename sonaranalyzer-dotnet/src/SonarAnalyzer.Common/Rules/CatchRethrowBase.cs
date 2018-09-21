/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class CatchRethrowBase<TCatchClause> : SonarDiagnosticAnalyzer
            where TCatchClause : SyntaxNode
    {
        internal const string DiagnosticId = "S2737";
        protected const string MessageFormat = "Add logic to this catch clause or eliminate it and rethrow the exception automatically.";

        protected abstract DiagnosticDescriptor Rule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        protected abstract IReadOnlyList<TCatchClause> GetCatches(SyntaxNode syntaxNode);

        protected abstract bool HasFilter(TCatchClause catchClause);

        protected abstract SyntaxNode GetDeclarationType(TCatchClause catchClause);

        protected void RaiseOnInvalidCatch(SyntaxNodeAnalysisContext context)
        {
            var catches = GetCatches(context.Node);
            var caughtExceptionTypes = new Lazy<List<INamedTypeSymbol>>(() =>
                ComputeExceptionTypesIfNeeded(catches, context.SemanticModel));
            var redundantCatches = new HashSet<TCatchClause>();

            // We handle differently catch clauses that just throw, but have
            // next catch clause that handles more basic exception _and_ does
            // more than just throw, because when removed they could change the
            // behavior of the method.
            var isIntermediate = false;

            for (var i = catches.Count - 1; i >= 0; i--)
            {
                var currentCatch = catches[i];
                if (ContainsOnlyThrow(currentCatch))
                {
                    if (isIntermediate)
                    {
                        if (!HasFilter(currentCatch) &&
                            !IsMoreSpecificTypeThanANotRedundantCatch(i, catches, caughtExceptionTypes.Value, redundantCatches))
                        {
                            redundantCatches.Add(currentCatch);
                        }
                    }
                    else
                    {
                        redundantCatches.Add(currentCatch);
                    }
                }
                else
                {
                    isIntermediate = true;
                }
            }

            foreach (var redundantCatch in redundantCatches)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, redundantCatch.GetLocation()));
            }
        }

        protected abstract bool ContainsOnlyThrow(TCatchClause currentCatch);

        private static bool IsMoreSpecificTypeThanANotRedundantCatch(int catchIndex, IReadOnlyList<TCatchClause> catches,
            List<INamedTypeSymbol> caughtExceptionTypes, ISet<TCatchClause> redundantCatches)
        {
            var currentType = caughtExceptionTypes[catchIndex];
            for (var i = catchIndex + 1; i < caughtExceptionTypes.Count; i++)
            {
                var followingType = caughtExceptionTypes[i];

                if (followingType == null ||
                    currentType.DerivesOrImplements(followingType))
                {
                    return !redundantCatches.Contains(catches[i]);
                }
            }
            return false;
        }

        private List<INamedTypeSymbol> ComputeExceptionTypesIfNeeded(IEnumerable<TCatchClause> catches,
            SemanticModel semanticModel)
        {
            return catches
                .Select(clause =>
                    {
                        var declarationType = GetDeclarationType(clause);
                        return declarationType != null
                            ? semanticModel.GetTypeInfo(declarationType).Type as INamedTypeSymbol
                            : null;
                    })
                .ToList();
        }
    }
}
