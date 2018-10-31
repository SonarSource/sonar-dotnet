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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class CSharpHotspotHelper : HotspotHelper
    {
        public CSharpHotspotHelper(IAnalyzerConfiguration analysisConfiguration,
            ImmutableArray<DiagnosticDescriptor> supportedDiagnostics)
            : base(analysisConfiguration, supportedDiagnostics)
        {
        }

        public override void TrackMethodInvocations(SonarAnalysisContext context, DiagnosticDescriptor rule,
            params MethodSignature[] trackedMethods)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(TrackInvocationExpression,
                            SyntaxKind.InvocationExpression);
                    }
                });

            void TrackInvocationExpression(SyntaxNodeAnalysisContext c)
            {
                if (IsTrackedMethod((InvocationExpressionSyntax)c.Node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                }
            }

            bool IsTrackedMethod(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
            {
                var identifier = invocation.Expression.GetIdentifier();
                return identifier != null &&
                    trackedMethods.Any(MethodSignatureHelper.IsSameMethod(identifier, semanticModel));
            }
        }

        public override void TrackPropertyAccess(SonarAnalysisContext context, DiagnosticDescriptor rule,
            params MethodSignature[] trackedProperties)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSyntaxNodeActionInNonGenerated(TrackMemberAccess,
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxKind.MemberBindingExpression,
                            SyntaxKind.IdentifierName);
                    }
                });

            void TrackMemberAccess(SyntaxNodeAnalysisContext c)
            {
                if (IsTrackedProperty((ExpressionSyntax)c.Node, c.SemanticModel))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                }
            }

            bool IsTrackedProperty(ExpressionSyntax expression, SemanticModel semanticModel)
            {
                // We register for both MemberAccess and IdentifierName and we want to
                // avoid raising two times for the same identifier.
                if (expression.IsKind(SyntaxKind.IdentifierName) &&
                    expression.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    return false;
                }

                var identifier = expression.GetIdentifier();
                return identifier != null &&
                    trackedProperties.Any(MethodSignatureHelper.IsSameProperty(identifier, semanticModel));
            }
        }
    }
}
