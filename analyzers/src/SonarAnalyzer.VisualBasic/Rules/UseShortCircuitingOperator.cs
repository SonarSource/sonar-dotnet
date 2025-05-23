﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class UseShortCircuitingOperator : UseShortCircuitingOperatorBase<SyntaxKind, BinaryExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        protected override string GetSuggestedOpName(BinaryExpressionSyntax node) =>
            OperatorNames[ShortCircuitingAlternative[node.Kind()]];

        protected override string GetCurrentOpName(BinaryExpressionSyntax node) =>
            OperatorNames[node.Kind()];

        protected override SyntaxToken GetOperator(BinaryExpressionSyntax expression) =>
            expression.OperatorToken;

        internal static readonly IDictionary<SyntaxKind, SyntaxKind> ShortCircuitingAlternative = new Dictionary<SyntaxKind, SyntaxKind>
        {
            { SyntaxKind.AndExpression, SyntaxKind.AndAlsoExpression },
            { SyntaxKind.OrExpression, SyntaxKind.OrElseExpression }
        }.ToImmutableDictionary();

        private static readonly IDictionary<SyntaxKind, string> OperatorNames = new Dictionary<SyntaxKind, string>
        {
            { SyntaxKind.AndExpression, "And" },
            { SyntaxKind.OrExpression, "Or" },
            { SyntaxKind.AndAlsoExpression, "AndAlso" },
            { SyntaxKind.OrElseExpression, "OrElse" },
        }.ToImmutableDictionary();

        protected override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => ImmutableArray.Create<SyntaxKind>(
            SyntaxKind.AndExpression,
            SyntaxKind.OrExpression);
    }
}
