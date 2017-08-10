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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<string> TrackedMethodNames = ImmutableHashSet.Create("AreEqual", "AreSame");

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.IsTest())
                    {
                        return;
                    }

                    var methodCall = (InvocationExpressionSyntax)c.Node;
                    if (!methodCall.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        return;
                    }

                    var methodCallExpression = (MemberAccessExpressionSyntax)methodCall.Expression;
                    if (!TrackedMethodNames.Contains(methodCallExpression.Name.Identifier.ValueText))
                    {
                        return;
                    }

                    var methodCallClassSymbol = c.SemanticModel
                        .GetSymbolInfo(methodCallExpression.Expression).Symbol as INamedTypeSymbol;
                    if (!methodCallClassSymbol.Is(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert))
                    {
                        return;
                    }

                    if (methodCall.ArgumentList.Arguments.Count >= 2 &&
                        !(methodCall.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax) &&
                        methodCall.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax)
                    {
                        var location = Location.Create(c.Node.SyntaxTree, new TextSpan(
                            methodCall.ArgumentList.Arguments[0].SpanStart,
                            methodCall.ArgumentList.Arguments[1].Span.End -
                                methodCall.ArgumentList.Arguments[0].SpanStart));
                        c.ReportDiagnostic(Diagnostic.Create(rule, location));
                    }
                }, SyntaxKind.InvocationExpression);
        }
    }
}
