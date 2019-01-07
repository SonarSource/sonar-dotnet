/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
        private const string MessageFormat = "Define a constant instead of using this literal '{0}' {1} times.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const int MinimumStringLength = 5;

        private const int ThresholdDefaultValue = 3;
        [RuleParameter("threshold", PropertyType.Integer, "Number of times a literal must be duplicated to trigger an issue.",
            ThresholdDefaultValue)]
        public int Threshold { get; set; } = ThresholdDefaultValue;

        protected override void Initialize(ParameterLoadingAnalysisContext context)
        {
            // Ideally we would like to report at assembly/project level for the primary and all string instances for secondary
            // locations. The problem is that this scenario is not yet supported on SonarQube side.
            // Hence the decision to do like other languages, at class-level
            context.RegisterSyntaxNodeActionInNonGenerated(
                ReportOnViolation,
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private void ReportOnViolation(SyntaxNodeAnalysisContext context)
        {
            if (context.Node.Ancestors().OfType<ClassDeclarationSyntax>().Any())
            {
                // Don't report on inner instances
                return;
            }

            var stringLiterals = context.Node
                .DescendantNodes(n => !n.IsKind(SyntaxKind.AttributeList))
                .Where(les => les.IsKind(SyntaxKind.StringLiteralExpression))
                .Cast<LiteralExpressionSyntax>();

            // Collect duplications
            var stringWithLiterals = new Dictionary<string, List<LiteralExpressionSyntax>>();
            foreach (var literal in stringLiterals)
            {
                // Remove leading and trailing double quotes
                var stringValue = ExtractStringContent(literal.Token.Text);

                if (stringValue != null &&
                    stringValue.Length >= MinimumStringLength &&
                    !IsMatchingMethodParameterName(literal))
                {
                    if (!stringWithLiterals.ContainsKey(stringValue))
                    {
                        stringWithLiterals[stringValue] = new List<LiteralExpressionSyntax>();
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
                        additionalLocations: item.Value.Skip(1).Select(x => x.GetLocation()),
                        messageArgs: new object[] { item.Key, item.Value.Count }));
                }
            }
        }

        private static bool IsMatchingMethodParameterName(LiteralExpressionSyntax literalExpression) =>
            literalExpression.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()
                ?.ParameterList
                ?.Parameters
                .Any(p => p.Identifier.ValueText == literalExpression.Token.ValueText)
                ?? false;

        private static string ExtractStringContent(string literal)
        {
            if (literal.StartsWith("@\""))
            {
                return literal.Substring(2, literal.Length - 3);
            }
            else
            {
                return literal.Substring(1, literal.Length - 2);
            }
        }
    }
}
