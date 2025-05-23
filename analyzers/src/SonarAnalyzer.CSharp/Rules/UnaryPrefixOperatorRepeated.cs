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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class UnaryPrefixOperatorRepeated :
        UnaryPrefixOperatorRepeatedBase<SyntaxKind, PrefixUnaryExpressionSyntax>
    {
        protected override DiagnosticDescriptor Rule { get; } =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        protected override ISet<SyntaxKind> SyntaxKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.LogicalNotExpression,
            SyntaxKind.BitwiseNotExpression,
        };

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            CSharpGeneratedCodeRecognizer.Instance;

        protected override SyntaxNode GetOperand(PrefixUnaryExpressionSyntax unarySyntax) =>
            unarySyntax.Operand;

        protected override SyntaxToken GetOperatorToken(PrefixUnaryExpressionSyntax unarySyntax) =>
            unarySyntax.OperatorToken;

        protected override bool SameOperators(PrefixUnaryExpressionSyntax expression1, PrefixUnaryExpressionSyntax expression2) =>
            expression1.OperatorToken.IsKind(expression2.OperatorToken.Kind());
    }
}
