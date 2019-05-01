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

using System;
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
    public sealed class DeliveringDebugFeaturesInProduction : DeliveringDebugFeaturesInProductionBase<SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager)
                .WithNotConfigurable();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(rule);

        public DeliveringDebugFeaturesInProduction()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal /*for testing*/ DeliveringDebugFeaturesInProduction(IAnalyzerConfiguration analyzerConfiguration)
        {
            InvocationTracker = new CSharpInvocationTracker(analyzerConfiguration, rule);
        }

        protected override InvocationCondition IsInvokedConditionally() =>
            (context) =>
            {
                var invocationStatement = context.Invocation.FirstAncestorOrSelf<StatementSyntax>();
                return invocationStatement != null &&
                    invocationStatement.Ancestors().Any(node => IsDevelopmentCheck(node, context.SemanticModel));
            };

        private bool IsDevelopmentCheck(SyntaxNode node, SemanticModel semanticModel)
        {
            if (!node.IsKind(SyntaxKind.IfStatement))
            {
                return false;
            }

            var condition = ((IfStatementSyntax)node).Condition.RemoveParentheses();

            if (condition != null && condition.IsKind(SyntaxKind.InvocationExpression))
            {
                return IsMatch(semanticModel, (InvocationExpressionSyntax)condition);
            }

            return false;
        }

        private static bool IsMatch(SemanticModel semanticModel, InvocationExpressionSyntax condition)
        {
            var methodName = condition.Expression.GetIdentifier()?.Identifier.ValueText;

            var methodSymbol = new Lazy<IMethodSymbol>(() => semanticModel.GetSymbolInfo(condition).Symbol as IMethodSymbol);

            return isDevelopmentMethod.IsMatch(methodName, methodSymbol);
        }
    }
}
