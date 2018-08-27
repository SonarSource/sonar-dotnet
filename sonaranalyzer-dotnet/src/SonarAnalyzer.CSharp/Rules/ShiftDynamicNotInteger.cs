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

using System;
using System.Collections.Immutable;
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
    public sealed class ShiftDynamicNotInteger : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3449";
        private const string MessageFormat = "Remove this erroneous shift, it will fail because '{0}' can't be implicitly converted to 'int'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithTwoParts<BinaryExpressionSyntax>(c.Node, b => b.Left, b => b.Right, c),
                SyntaxKind.LeftShiftExpression,
                SyntaxKind.RightShiftExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithTwoParts<AssignmentExpressionSyntax>(c.Node, b => b.Left, b => b.Right, c),
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression);
        }

        private static void CheckExpressionWithTwoParts<T>(SyntaxNode node, Func<T, ExpressionSyntax> leftSelector, Func<T, ExpressionSyntax> rightSelector,
            SyntaxNodeAnalysisContext context)
            where T : SyntaxNode
        {
            var nodeWithTwoSides = (T)node;
            var left = leftSelector(nodeWithTwoSides);
            var right = rightSelector(nodeWithTwoSides);

            if (IsDynamic(left, context.SemanticModel) &&
                !MightBeConvertibleToInt(right, context.SemanticModel, out var typeOfRight))
            {
                var typeInMessage = GetTypeNameForMessage(right, typeOfRight, context.SemanticModel);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, right.GetLocation(),
                    typeInMessage));
            }
        }

        private static string GetTypeNameForMessage(ExpressionSyntax expression, ITypeSymbol typeOfRight, SemanticModel semanticModel)
        {
            var constValue = semanticModel.GetConstantValue(expression);
            return constValue.HasValue && constValue.Value == null
                ? "null"
                : typeOfRight.ToMinimalDisplayString(semanticModel, expression.SpanStart);
        }

        private static bool MightBeConvertibleToInt(ExpressionSyntax expression, SemanticModel semanticModel, out ITypeSymbol type)
        {
            type = semanticModel.GetTypeInfo(expression).Type;
            if (type is IErrorTypeSymbol)
            {
                return true;
            }

            var intType = semanticModel.Compilation.GetTypeByMetadataName("System.Int32");
            if (intType == null)
            {
                return false;
            }

            var conversion = semanticModel.ClassifyConversion(expression, intType);
            return conversion.Exists && (conversion.IsIdentity || conversion.IsImplicit);
        }

        private static bool IsDynamic(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var type = semanticModel.GetTypeInfo(expression).Type;
            return type != null && type.TypeKind == TypeKind.Dynamic;
        }
    }
}
