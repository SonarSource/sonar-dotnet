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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotTestThisWithIsOperator : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3060";
        private const string MessageFormat = "Offload the code that's conditional on this 'is' test to the appropriate subclass and remove the test.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeIsExpression, SyntaxKind.IsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeIsPatternExpression, SyntaxKindEx.IsPatternExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeSwitchExpression, SyntaxKindEx.SwitchExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
        }

        private static void AnalyzeIsExpression(SyntaxNodeAnalysisContext context)
        {
            if (((BinaryExpressionSyntax)context.Node).Left.RemoveParentheses() is ThisExpressionSyntax)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private static void AnalyzeIsPatternExpression(SyntaxNodeAnalysisContext context)
        {
            var isPatternExpression = (IsPatternExpressionSyntaxWrapper)context.Node;
            if (isPatternExpression.Expression.RemoveParentheses() is ThisExpressionSyntax
                && ContainsTypeCheckInPattern(isPatternExpression))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
        {
            var switchStatement = (SwitchStatementSyntax)context.Node;
            if (switchStatement.Expression.RemoveParentheses() is ThisExpressionSyntax
                && (ContainsTypeCheckInPattern(switchStatement)
                    || ContainsTypeCheckInCaswSwitchLabel(switchStatement)))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private static void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext context)
        {
            var switchExpression = (SwitchExpressionSyntaxWrapper)context.Node;
            if (switchExpression.GoverningExpression.RemoveParentheses() is ThisExpressionSyntax
                && ContainsTypeCheckInPattern(switchExpression))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, context.Node.GetLocation()));
            }
        }

        private static bool ContainsTypeCheckInCaswSwitchLabel(SwitchStatementSyntax switchStatement) =>
            switchStatement.Sections.Any(x => x.Labels.Any(x => x is CaseSwitchLabelSyntax caseSwitchLabel
                                                                && caseSwitchLabel.Value.IsKind(SyntaxKind.IdentifierName)));

        private static bool ContainsTypeCheckInPattern(SyntaxNode syntaxNode) =>
            syntaxNode.DescendantNodes()
                      .Where(x => x.IsAnyKind(SyntaxKindEx.ConstantPattern, SyntaxKindEx.DeclarationPattern, SyntaxKindEx.RecursivePattern))
                      .Any(x => IsTypeCheckOnThis(x));

        private static bool IsTypeCheckOnThis(SyntaxNode pattern)
        {
            if (ConstantPatternSyntaxWrapper.IsInstance(pattern)
                && (ConstantPatternSyntaxWrapper)pattern is var constantPattern)
            {
                return constantPattern.Expression.IsKind(SyntaxKind.IdentifierName)
                       && IsNotInSubPattern(pattern);
            }
            else if (DeclarationPatternSyntaxWrapper.IsInstance(pattern))
            {
                return IsNotInSubPattern(pattern);
            }
            else if (RecursivePatternSyntaxWrapper.IsInstance(pattern)
                     && (RecursivePatternSyntaxWrapper)pattern is var recursivePattern)
            {
                return recursivePattern.Type != null && IsNotInSubPattern(pattern);
            }

            return false;
        }

        private static bool IsNotInSubPattern(SyntaxNode node) =>
            node.FirstAncestorOrSelf<SyntaxNode>(x => x.IsAnyKind(SyntaxKindEx.IsPatternExpression,
                                                                  SyntaxKindEx.SwitchExpression,
                                                                  SyntaxKind.SwitchStatement,
                                                                  SyntaxKindEx.Subpattern)) is var containingNode
            && !containingNode.IsKind(SyntaxKindEx.Subpattern);
    }
}
