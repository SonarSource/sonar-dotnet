/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TrackNotImplementedException : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3717";
        private const string MessageFormat = "Implement this method or throw 'NotSupportedException' instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var throwStatement = (ThrowStatementSyntax)c.Node;
                    if (throwStatement.Expression == null)
                    {
                        return;
                    }

                    ReportDiagnostic(c, throwStatement.Expression, throwStatement);
                },
                SyntaxKind.ThrowStatement);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var creationExpression = (ObjectCreationExpressionSyntax)c.Node;
                    var throwExpression = creationExpression.Parent;

                    if (throwExpression.Kind() != SyntaxKindEx.ThrowExpression)
                    {
                        return;
                    }

                    ReportDiagnostic(c, creationExpression, throwExpression);
                },
                SyntaxKind.ObjectCreationExpression);
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext c, ExpressionSyntax expression, SyntaxNode syntax)
        {
            if (c.SemanticModel.GetTypeInfo(expression).Type.Is(KnownType.System_NotImplementedException))
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, syntax.GetLocation()));
            }
        }
    }
}
