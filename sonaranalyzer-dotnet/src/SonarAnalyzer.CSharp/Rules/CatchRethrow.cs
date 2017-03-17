/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class CatchRethrow : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2737";
        private const string MessageFormat = @"Add logic to this catch clause or eliminate it and rethrow the exception automatically.";
        private const IdeVisibility ideVisibility = IdeVisibility.Hidden;

        private static readonly BlockSyntax ThrowBlock = SyntaxFactory.Block(SyntaxFactory.ThrowStatement());

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, ideVisibility, RspecStrings.ResourceManager);

        protected sealed override DiagnosticDescriptor Rule => rule;

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(RaiseOnInvalidCatch, SyntaxKind.TryStatement);
        }

        private void RaiseOnInvalidCatch(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;
            var catches = tryStatement.Catches.ToList();
            var caughtExceptionTypes = new Lazy<List<INamedTypeSymbol>>(() =>
                ComputeExceptionTypesIfNeeded(catches, context.SemanticModel));
            var redundantCatches = new HashSet<CatchClauseSyntax>();
            var isIntermediate = false;

            for (int i = catches.Count - 1; i >= 0; i--)
            {
                var currentCatch = catches[i];
                if (!EquivalenceChecker.AreEquivalent(currentCatch.Block, ThrowBlock))
                {
                    isIntermediate = true;
                    continue;
                }

                if (!isIntermediate)
                {
                    redundantCatches.Add(currentCatch);
                    continue;
                }

                if (currentCatch.Filter != null)
                {
                    continue;
                }

                if (!IsMoreSpecificTypeThanANotRedundantCatch(i, catches, caughtExceptionTypes.Value, redundantCatches))
                {
                    redundantCatches.Add(currentCatch);
                }
            }

            foreach (var redundantCatch in redundantCatches)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, redundantCatch.GetLocation()));
            }
        }

        private static bool IsMoreSpecificTypeThanANotRedundantCatch(int catchIndex, List<CatchClauseSyntax> catches,
            List<INamedTypeSymbol> caughtExceptionTypes, ISet<CatchClauseSyntax> redundantCatches)
        {
            var currentType = caughtExceptionTypes[catchIndex];
            for (int i = catchIndex + 1; i < caughtExceptionTypes.Count; i++)
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

        private static List<INamedTypeSymbol> ComputeExceptionTypesIfNeeded(IEnumerable<CatchClauseSyntax> catches,
            SemanticModel semanticModel)
        {
            return catches
                .Select(clause =>
                    clause.Declaration?.Type != null
                        ? semanticModel.GetTypeInfo(clause.Declaration?.Type).Type as INamedTypeSymbol
                        : null)
                .ToList();
        }
    }
}
