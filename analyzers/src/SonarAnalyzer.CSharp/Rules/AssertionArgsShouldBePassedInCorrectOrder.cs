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

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3415";
        private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, actual value.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly IDictionary<string, ImmutableArray<KnownType>> MethodsWithType = new Dictionary<string, ImmutableArray<KnownType>>
        {
            ["AreEqual"]    = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            ["AreNotEqual"] = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            ["AreSame"]     = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            ["AreNotSame"]  = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert, KnownType.NUnit_Framework_Assert),
            ["Equal"]       = ImmutableArray.Create(KnownType.Xunit_Assert),
            ["Same"]        = ImmutableArray.Create(KnownType.Xunit_Assert),
            ["NotSame"]     = ImmutableArray.Create(KnownType.Xunit_Assert)
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var methodCall = (InvocationExpressionSyntax)c.Node;
                if (!methodCall.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    || methodCall.ArgumentList.Arguments.Count < 2)
                {
                    return;
                }

                var firstArgument = methodCall.ArgumentList.Arguments[0];
                var secondArgument = methodCall.ArgumentList.Arguments[1];
                if (firstArgument.Expression is LiteralExpressionSyntax
                    || secondArgument.Expression is not LiteralExpressionSyntax)
                {
                    return;
                }

                var methodCallExpression = (MemberAccessExpressionSyntax)methodCall.Expression;

                var methodKnownTypes = MethodsWithType.GetValueOrDefault(methodCallExpression.Name.Identifier.ValueText);
                if (methodKnownTypes == null)
                {
                    return;
                }

                var symbolInfo = c.SemanticModel.GetSymbolInfo(methodCallExpression.Expression).Symbol;
                var isAnyTrackedAssertType = (symbolInfo as INamedTypeSymbol).IsAny(methodKnownTypes);
                if (!isAnyTrackedAssertType)
                {
                    return;
                }

                c.ReportIssue(Diagnostic.Create(Rule, firstArgument.CreateLocation(secondArgument)));
            },
            SyntaxKind.InvocationExpression);
    }
}
