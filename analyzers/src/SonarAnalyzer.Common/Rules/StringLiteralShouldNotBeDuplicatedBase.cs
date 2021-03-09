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

        private readonly DiagnosticDescriptor rule;

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract TSyntaxKind[] SyntaxKinds { get; }

        protected abstract bool IsMatchingMethodParameterName(TLiteralExpressionSyntax literalExpression);
        protected abstract bool IsInnerInstance(SyntaxNodeAnalysisContext context);
        protected abstract IEnumerable<TLiteralExpressionSyntax> RetrieveLiteralExpressions(SyntaxNode node);
        protected abstract string GetLiteralValue(TLiteralExpressionSyntax literal);

        [RuleParameter("threshold", PropertyType.Integer, "Number of times a literal must be duplicated to trigger an issue.", ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected StringLiteralShouldNotBeDuplicatedBase(System.Resources.ResourceManager resourceManager) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, resourceManager, isEnabledByDefault: false);

        protected override void Initialize(AdditionalCompilationStartActionAnalysisContext context) =>
            // Ideally we would like to report at assembly/project level for the primary and all string instances for secondary
            // locations. The problem is that this scenario is not yet supported on SonarQube side.
            // Hence the decision to do like other languages, at class-level
            context.RegisterSyntaxNodeActionInNonGenerated(GeneratedCodeRecognizer, ReportOnViolation, SyntaxKinds);

        private void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            if (IsInnerInstance(context))
            {
                // Don't report on inner instances
                return;
            }

            var stringLiterals = RetrieveLiteralExpressions(context.Node);

            // Collect duplications
            var stringWithLiterals = new Dictionary<string, List<TLiteralExpressionSyntax>>();
            foreach (var literal in stringLiterals)
            {
                // Remove leading and trailing double quotes
                var stringValue = ExtractStringContent(GetLiteralValue(literal));

                if (stringValue != null
                    && stringValue.Length >= MinimumStringLength
                    && !IsMatchingMethodParameterName(literal))
                {
                    if (!stringWithLiterals.ContainsKey(stringValue))
                    {
                        stringWithLiterals[stringValue] = new List<TLiteralExpressionSyntax>();
                    }

                    stringWithLiterals[stringValue].Add(literal);
                }
            }

            // Report duplications
            foreach (var item in stringWithLiterals)
            {
                if (item.Value.Count > Threshold)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, item.Value[0].GetLocation(),
                        item.Value.Skip(1).Select(x => x.GetLocation()),
                        item.Key, item.Value.Count ));
                }
            }
        }

        private static string ExtractStringContent(string literal) =>
            literal.StartsWith("@\"") ? literal.Substring(2, literal.Length - 3) : literal.Substring(1, literal.Length - 2);
    }
}
