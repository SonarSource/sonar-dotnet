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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class StringLiteralShouldNotBeDuplicated : ParameterLoadingDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1192";
        private const string MessageFormat = "Define a constant instead of using the literal '{0}' {1} times.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private const int MinimumStringLength = 5;

        private const int ThresholdDefaultValue = 3;
        [RuleParameter("threshold", PropertyType.Integer, "Number of times a literal must be duplicated to trigger an issue.",
            ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                csac =>
                {
                    if (csac.Compilation.IsTest())
                    {
                        return;
                    }

                    var stringWithLiterals = new Dictionary<string, List<LiteralExpressionSyntax>>();

                    csac.RegisterSyntaxNodeActionInNonGenerated(
                        snac => CollectDuplications((LiteralExpressionSyntax)snac.Node, stringWithLiterals),
                        SyntaxKind.StringLiteralExpression);

                    csac.RegisterCompilationEndAction(cac => ReportDuplicates(cac, stringWithLiterals));
                });
        }

        private void CollectDuplications(LiteralExpressionSyntax stringLiteral,
            Dictionary<string, List<LiteralExpressionSyntax>> stringWithLiterals)
        {
            var stringValue = stringLiteral.Token.ValueText;

            if (stringValue != null &&
                stringValue.Length >= MinimumStringLength &&
                !IsMatchingMethodParameterName(stringLiteral))
            {
                if (!stringWithLiterals.ContainsKey(stringValue))
                {
                    stringWithLiterals[stringValue] = new List<LiteralExpressionSyntax>();
                }

                stringWithLiterals[stringValue].Add(stringLiteral);
            }
        }

        private void ReportDuplicates(CompilationAnalysisContext compilationAnalysisContext,
            Dictionary<string, List<LiteralExpressionSyntax>> stringWithLiterals)
        {
            foreach (var item in stringWithLiterals)
            {
                if (item.Value.Count > Threshold)
                {
                    // Report issues as project-level
                    compilationAnalysisContext.ReportDiagnosticWhenActive(Diagnostic.Create(rule, null,
                        additionalLocations: item.Value.Select(x => x.GetLocation()).OrderBy(x => x.SourceSpan),
                        messageArgs: new object[] { item.Key, item.Value.Count }));
                }
            }
        }

        private bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
                ?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.ValueText == literalExpression.Token.ValueText)
                ?? false;
    }
}
