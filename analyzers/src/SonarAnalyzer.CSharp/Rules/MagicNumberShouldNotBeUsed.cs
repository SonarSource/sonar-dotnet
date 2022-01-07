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

using System;
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
    public sealed class MagicNumberShouldNotBeUsed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S109";
        private const string MessageFormat = "Assign this magic number '{0}' to a well-named (variable|constant), and use the (variable|constant) instead.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<string> NotConsideredAsMagicNumbers = new HashSet<string> { "-1", "0", "1" };

        private static readonly string[] AcceptedNamesForSingleDigitComparison = { "size", "count", "length" };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var literalExpression = (LiteralExpressionSyntax)c.Node;

                    if (!IsExceptionToTheRule(literalExpression))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, literalExpression.GetLocation(),
                            literalExpression.Token.ValueText));
                    }
                },
                SyntaxKind.NumericLiteralExpression);

        private static bool IsExceptionToTheRule(LiteralExpressionSyntax literalExpression) =>
            NotConsideredAsMagicNumbers.Contains(literalExpression.Token.ValueText)
            || literalExpression.FirstAncestorOrSelf<VariableDeclarationSyntax>() != null
            || literalExpression.FirstAncestorOrSelf<ParameterSyntax>() != null
            || literalExpression.FirstAncestorOrSelf<EnumMemberDeclarationSyntax>() != null
            || literalExpression.FirstAncestorOrSelf<MethodDeclarationSyntax>()?.Identifier.ValueText == nameof(object.GetHashCode)
            || literalExpression.FirstAncestorOrSelf<PragmaWarningDirectiveTriviaSyntax>() != null
            || IsInsideProperty(literalExpression)
            || IsSingleDigitInToleratedComparisons(literalExpression)
            || IsToleratedArgument(literalExpression);

        // Inside property we consider magic numbers as exceptions in the following cases:
        //   - A {get; set;} = MAGIC_NUMBER
        //   - A { get { return MAGIC_NUMBER; } }
        private static bool IsInsideProperty(SyntaxNode node)
        {
            if (node.FirstAncestorOrSelf<PropertyDeclarationSyntax>() == null)
            {
                return false;
            }
            var parent = node.Parent;
            return parent is ReturnStatementSyntax || parent is EqualsValueClauseSyntax;
        }

        private static bool IsSingleDigitInToleratedComparisons(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Parent is BinaryExpressionSyntax binaryExpression
            && IsSingleDigit(literalExpression.Token.ValueText)
            && ToStringContainsAnyAcceptedNames(binaryExpression);

        private static bool IsToleratedArgument(LiteralExpressionSyntax literalExpression) =>
            IsToleratedMethodArgument(literalExpression)
            || IsSingleOrNamedAttributeArgument(literalExpression);

        // Named argument or constructor argument.
        private static bool IsToleratedMethodArgument(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Parent is ArgumentSyntax arg
            && (arg.NameColon is not null || arg.Parent.Parent is ObjectCreationExpressionSyntax);

        private static bool IsSingleOrNamedAttributeArgument(LiteralExpressionSyntax literalExpression) =>
            literalExpression.Parent is AttributeArgumentSyntax arg
            && (arg.NameColon is not null
                || arg.NameEquals is not null
                || (arg.Parent is AttributeArgumentListSyntax argList && argList.Arguments.Count == 1));

        private static bool IsSingleDigit(string text) => byte.TryParse(text, out var result) && result <= 9;

        private static bool ToStringContainsAnyAcceptedNames(SyntaxNode syntaxNode)
        {
            var toString = syntaxNode.ToString().ToLower();
            return AcceptedNamesForSingleDigitComparison.Any(x => toString.Contains(x));
        }
    }
}
