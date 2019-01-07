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
    public sealed class ShiftDynamicNotInteger : ShiftDynamicNotIntegerBase<ExpressionSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override DiagnosticDescriptor Rule { get; } = rule;

        protected override bool ShouldRaise(SemanticModel semanticModel, ExpressionSyntax left, ExpressionSyntax right) =>
            IsDynamic(left, semanticModel) &&
            !IsConvertibleToInt(right, semanticModel);

        protected override bool CanBeConvertedTo(ExpressionSyntax expression, ITypeSymbol type, SemanticModel semanticModel)
        {
            var conversion = semanticModel.ClassifyConversion(expression, type);
            return conversion.Exists && (conversion.IsIdentity || conversion.IsImplicit);
        }

        private static bool IsDynamic(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var type = semanticModel.GetTypeInfo(expression).Type;
            return type != null && type.TypeKind == TypeKind.Dynamic;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithTwoParts<BinaryExpressionSyntax>(c, b => b.Left, b => b.Right),
                SyntaxKind.LeftShiftExpression,
                SyntaxKind.RightShiftExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckExpressionWithTwoParts<AssignmentExpressionSyntax>(c, b => b.Left, b => b.Right),
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression);
        }
    }
}
