/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class StringLiteralShouldNotBeDuplicatedBase<TSyntaxKind, TLiteralExpressionSyntax> : ParameterLoadingDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TLiteralExpressionSyntax : SyntaxNode
    {
        protected const string DiagnosticId = "S1192";
        private const string MessageFormat = "Define a constant instead of using this literal '{0}' {1} times.";
        private const int MinimumStringLength = 5;
        private const int ThresholdDefaultValue = 3;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private readonly DiagnosticDescriptor rule;

        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract bool IsMatchingMethodParameterName(TLiteralExpressionSyntax literalExpression);
        protected abstract bool IsInnerInstance(SyntaxNodeAnalysisContext context);
        protected abstract IEnumerable<TLiteralExpressionSyntax> RetrieveLiteralExpressions(SyntaxNode node);
        protected abstract SyntaxToken GetLiteralToken(TLiteralExpressionSyntax literal);

        [RuleParameter("threshold", PropertyType.Integer, "Number of times a literal must be duplicated to trigger an issue.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected StringLiteralShouldNotBeDuplicatedBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources, false);

        protected override void Initialize(ParameterLoadingAnalysisContext context) =>
            // Ideally we would like to report at assembly/project level for the primary and all string instances for secondary
            // locations. The problem is that this scenario is not yet supported on SonarQube side.
            // Hence the decision to do like other languages, at class-level
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, ReportOnViolation, SyntaxKinds);

        protected virtual bool IsNamedTypeOrTopLevelMain(SyntaxNodeAnalysisContext context) =>
            IsNamedType(context);

        protected static bool IsNamedType(SyntaxNodeAnalysisContext context) =>
            context.ContainingSymbol.Kind == SymbolKind.NamedType;

        private void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            if (!IsNamedTypeOrTopLevelMain(context) || IsInnerInstance(context))
            {
                // Don't report on inner instances
                return;
            }

            var stringLiterals = RetrieveLiteralExpressions(context.Node);
            var stringWithLiterals = from literal in stringLiterals
                                     let literalToken = GetLiteralToken(literal)
                                     let literalValue = literalToken.ValueText
                                     where
                                        literalToken.ValueText is { Length: >= MinimumStringLength }
                                        && !IsMatchingMethodParameterName(literal)
                                     group literalToken by literalValue;

            // Report duplications
            foreach (var item in stringWithLiterals)
            {
                var duplicates = item.ToList();
                if (duplicates.Count > Math.Max(Threshold, 0))
                {
                    var firstToken = duplicates[0];
                    context.ReportIssue(Diagnostic.Create(rule, firstToken.GetLocation(),
                        duplicates.Skip(1).Select(x => x.GetLocation()),
                        item.Key, duplicates.Count));
                }
            }
        }
    }
}
