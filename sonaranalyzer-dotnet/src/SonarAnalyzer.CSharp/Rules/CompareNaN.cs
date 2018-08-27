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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class CompareNaN : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2688";
        private const string MessageFormat = "Use {0}.IsNaN() instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var binaryExpressionSyntax = (BinaryExpressionSyntax)c.Node;

                    if (TryGetFloatingPointType(binaryExpressionSyntax.Left, c.SemanticModel, out var floatingPointType) ||
                        TryGetFloatingPointType(binaryExpressionSyntax.Right, c.SemanticModel, out floatingPointType))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, binaryExpressionSyntax.GetLocation(), floatingPointType.TypeName));
                    }
                },
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression);
        }

        private static bool TryGetFloatingPointType(ExpressionSyntax expression, SemanticModel semanticModel, out KnownType floatingPointType)
        {
            floatingPointType = null;

            if (!(expression is MemberAccessExpressionSyntax memberAccess))
            {
                return false;
            }

            var fieldSymbol = semanticModel.GetSymbolInfo(memberAccess).Symbol as IFieldSymbol;
            if (fieldSymbol?.Name != nameof(double.NaN))
            {
                return false;
            }

            floatingPointType = KnownType.FloatingPointNumbers.FirstOrDefault(fieldSymbol.Type.Is);

            return floatingPointType != null;
        }
    }
}
