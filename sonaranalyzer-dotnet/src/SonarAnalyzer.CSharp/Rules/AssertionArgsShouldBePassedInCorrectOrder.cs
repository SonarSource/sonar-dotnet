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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class AssertionArgsShouldBePassedInCorrectOrder : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3415";
        private const string MessageFormat = "Make sure these 2 arguments are in the correct order: expected value, " +
            "actual value.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly IDictionary<string, ImmutableArray<KnownType>> methodsWithType = new Dictionary<string, ImmutableArray<KnownType>>
        {
            ["AreEqual"] = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert,
                KnownType.NUnit_Framework_Assert),
            ["AreSame"] = ImmutableArray.Create(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert,
                KnownType.NUnit_Framework_Assert),
            ["Equal"] = ImmutableArray.Create(KnownType.Xunit_Assert),
            ["Same"] = ImmutableArray.Create(KnownType.Xunit_Assert)
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodCall = (InvocationExpressionSyntax)c.Node;
                    if (!methodCall.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
                        methodCall.ArgumentList.Arguments.Count < 2)
                    {
                        return;
                    }

                    var methodCallExpression = (MemberAccessExpressionSyntax)methodCall.Expression;

                    var methodKnownTypes = methodsWithType.GetValueOrDefault(methodCallExpression.Name.Identifier.ValueText);
                    if (methodKnownTypes == null)
                    {
                        return;
                    }

                    var isAnyTrackedAssertType = (c.SemanticModel.GetSymbolInfo(methodCallExpression.Expression).Symbol
                        as INamedTypeSymbol).IsAny(methodKnownTypes);
                    if (!isAnyTrackedAssertType)
                    {
                        return;
                    }

                    var firstArgument = methodCall.ArgumentList.Arguments[0];
                    var secondArgument = methodCall.ArgumentList.Arguments[1];

                    if (!(firstArgument.Expression is LiteralExpressionSyntax) &&
                        secondArgument.Expression is LiteralExpressionSyntax)
                    {
                        var location = Location.Create(c.Node.SyntaxTree, new TextSpan(firstArgument.SpanStart,
                            secondArgument.Span.End - firstArgument.SpanStart));
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
                    }
                }, SyntaxKind.InvocationExpression);
        }
    }
}
