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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class UnaryPrefixOperatorRepeated :
        UnaryPrefixOperatorRepeatedBase<SyntaxKind, UnaryExpressionSyntax>
    {
        protected override ISet<SyntaxKind> SyntaxKinds { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.NotExpression
        };

        protected override DiagnosticDescriptor Rule { get; } =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            Helpers.VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        protected override SyntaxNode GetOperand(UnaryExpressionSyntax unarySyntax) =>
             unarySyntax.Operand;

        protected override SyntaxToken GetOperatorToken(UnaryExpressionSyntax unarySyntax) =>
            unarySyntax.OperatorToken;

        protected override bool SameOperators(UnaryExpressionSyntax expression1, UnaryExpressionSyntax expression2) =>
            expression1.OperatorToken.IsKind(expression2.OperatorToken.Kind());
    }
}
